using System.Text;
using System.Text.Json;
using ConexaoSolidaria.Domain.Events;
using RabbitMQ.Client;

namespace Campanhas.Api.Messaging;

public interface IDoacaoEventPublisher
{
    void Publish(DoacaoRecebidaEvent @event);
}

public sealed class RabbitMqDoacaoEventPublisher : IDoacaoEventPublisher, IDisposable
{
    public const string ExchangeName = "conexao.doacoes";
    public const string RoutingKey = "doacao.recebida";
    public const string QueueName = "doacao.recebida.queue";

    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqDoacaoEventPublisher> _logger;
    private readonly object _sync = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqDoacaoEventPublisher(IConfiguration configuration, ILogger<RabbitMqDoacaoEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Publish(DoacaoRecebidaEvent @event)
    {
        EnsureConnected();
        var payload = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(payload);
        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        _channel.BasicPublish(ExchangeName, RoutingKey, props, body);
        _logger.LogInformation("Evento DoacaoRecebida publicado: {DoacaoId}", @event.DoacaoId);
    }

    private void EnsureConnected()
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        lock (_sync)
        {
            if (_channel is { IsOpen: true })
            {
                return;
            }

            _channel?.Dispose();
            _connection?.Dispose();

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
            for (var attempt = 1; attempt <= 10; attempt++)
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
                    _logger.LogWarning(ex, "Falha ao conectar no RabbitMQ (tentativa {Attempt}/10)", attempt);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }

            throw new InvalidOperationException("Não foi possível conectar ao RabbitMQ.", last);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
