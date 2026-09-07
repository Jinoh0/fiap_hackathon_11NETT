using System.Text;
using System.Text.Json;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Events;
using Doacoes.Worker.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Doacoes.Worker.Messaging;

public class DoacaoConsumerService : BackgroundService
{
    public const string ExchangeName = "conexao.doacoes";
    public const string RoutingKey = "doacao.recebida";
    public const string QueueName = "doacao.recebida.queue";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DoacaoConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public DoacaoConsumerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DoacaoConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<DoacaoRecebidaEvent>(json)
                    ?? throw new InvalidOperationException("Payload inválido.");

                await ProcessAsync(evt, stoppingToken);
                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar mensagem de doação.");
                _channel!.BasicNack(ea.DeliveryTag, false, requeue: true);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        };

        _channel!.BasicQos(0, 1, false);
        _channel.BasicConsume(QueueName, autoAck: false, consumer);
        _logger.LogInformation("Worker consumindo fila {Queue}", QueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task ProcessAsync(DoacaoRecebidaEvent evt, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var doacao = await db.Doacoes.FirstOrDefaultAsync(d => d.Id == evt.DoacaoId, ct);
        if (doacao is null)
        {
            _logger.LogWarning("Doação {DoacaoId} não encontrada.", evt.DoacaoId);
            await tx.RollbackAsync(ct);
            return;
        }

        if (doacao.Status == DoacaoStatus.Processada)
        {
            await tx.CommitAsync(ct);
            return;
        }

        var campanha = await db.Campanhas.FirstOrDefaultAsync(c => c.Id == evt.CampanhaId, ct);
        if (campanha is null)
        {
            doacao.Status = DoacaoStatus.Falhou;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            _logger.LogWarning("Campanha {CampanhaId} não encontrada para doação {DoacaoId}.", evt.CampanhaId, evt.DoacaoId);
            return;
        }

        campanha.ValorArrecadado += evt.ValorDoacao;
        campanha.AtualizadoEm = DateTime.UtcNow;
        doacao.Status = DoacaoStatus.Processada;
        doacao.ProcessadoEm = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        WorkerMetrics.DoacoesProcessadas.Inc();
        _logger.LogInformation(
            "Doação {DoacaoId} processada. Campanha {CampanhaId} arrecadou {Valor}. Total={Total}",
            evt.DoacaoId, evt.CampanhaId, evt.ValorDoacao, campanha.ValorArrecadado);
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };

        Exception? last = null;
        for (var attempt = 1; attempt <= 30 && !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
                return;
            }
            catch (Exception ex)
            {
                last = ex;
                _logger.LogWarning(ex, "Aguardando RabbitMQ (tentativa {Attempt}/30)...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }

        throw new InvalidOperationException("Worker não conseguiu conectar ao RabbitMQ.", last);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
