using DonationWorker.Consumers;
using DonationWorker.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

// EF Core - SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DoacaoRecebidaConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });
        cfg.ConfigureEndpoints(ctx);
    });
});

// Prometheus - expoe /metrics na porta 9091
builder.Services.AddMetricServer(options => options.Port = 9091);

var host = builder.Build();

// Aplica migrations automaticamente ao iniciar
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

host.Run();
