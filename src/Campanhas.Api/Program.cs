using System.Text;
using Campanhas.Api.Auth;
using Campanhas.Api.Data;
using Campanhas.Api.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conexão Solidária API",
        Version = "v1",
        Description = "MVP da plataforma Conexão Solidária — campanhas, doadores e doações assíncronas."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IDoacaoEventPublisher, RabbitMqDoacaoEventPublisher>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
var rabbitUri = new Uri(
    $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:{builder.Configuration["RabbitMQ:Port"]}/");
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Default")!)
    .AddRabbitMQ(rabbitConnectionString: rabbitUri.ToString());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var attempt = 1; attempt <= 20; attempt++)
    {
        try
        {
            await DbSeeder.SeedAsync(db);
            break;
        }
        catch (Exception ex) when (attempt < 20)
        {
            app.Logger.LogWarning(ex, "Aguardando infraestrutura (tentativa {Attempt}/20)...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpMetrics();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

app.Run();

public partial class Program;
