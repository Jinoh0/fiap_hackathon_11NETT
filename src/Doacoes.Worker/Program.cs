using Doacoes.Worker.Data;
using Doacoes.Worker.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<WorkerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHostedService<DoacaoConsumerService>();
builder.Services.AddHostedService<MetricsHttpServer>();

var host = builder.Build();

await host.RunAsync();
