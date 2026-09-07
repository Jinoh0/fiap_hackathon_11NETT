using Doacoes.Worker.Data;
using Doacoes.Worker.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<WorkerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHostedService<DoacaoConsumerService>();
builder.Services.AddHostedService<MetricsHttpServer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
    for (var attempt = 1; attempt <= 20; attempt++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            break;
        }
        catch (Exception ex) when (attempt < 20)
        {
            host.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Startup")
                .LogWarning(ex, "Aguardando PostgreSQL (tentativa {Attempt}/20)...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

await host.RunAsync();
