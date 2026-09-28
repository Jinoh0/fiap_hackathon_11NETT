using Campanhas.Api.Services;

namespace Campanhas.Api.BackgroundJobs;

public class CloseExpiredCampaignsJob(IServiceScopeFactory scopeFactory, ILogger<CloseExpiredCampaignsJob> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await FecharAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FecharAsync(stoppingToken);
        }
    }

    private async Task FecharAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var campanhas = scope.ServiceProvider.GetRequiredService<CampanhaService>();
            var fechadas = await campanhas.FecharExpiradasAsync(ct);
            if (fechadas > 0)
            {
                logger.LogInformation("Job fechou {Count} campanha(s) com DataFim vencida.", fechadas);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao fechar campanhas expiradas.");
        }
    }
}
