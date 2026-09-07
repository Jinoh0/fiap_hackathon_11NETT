using Prometheus;

namespace Doacoes.Worker.Messaging;

public static class WorkerMetrics
{
    public static readonly Counter DoacoesProcessadas = Metrics.CreateCounter(
        "doacoes_processadas_total",
        "Total de doações processadas pelo worker");
}

public class MetricsHttpServer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MetricsHttpServer> _logger;
    private MetricServer? _server;

    public MetricsHttpServer(IConfiguration configuration, ILogger<MetricsHttpServer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = int.Parse(_configuration["Metrics:Port"] ?? "8081");
        _server = new MetricServer(port: port);
        _server.Start();
        _logger.LogInformation("Métricas do Worker em http://0.0.0.0:{Port}/metrics", port);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _server?.Stop();
        return base.StopAsync(cancellationToken);
    }
}
