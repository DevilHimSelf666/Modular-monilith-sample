using Inventory.Infrastructure.Consumers;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Inventory.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessageTransportOptions>(configuration.GetSection("Messaging"));

        services.AddDbContext<InventoryDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("InventoryDb") ?? throw new InvalidOperationException("Connection string 'InventoryDb' not found.");
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", InventoryDbContext.Schema));
        });

        services.AddScoped<IReservationService, ReservationService>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.AddConsumer<ProcurementCreatedConsumer>();

            busConfigurator.AddEntityFrameworkOutbox<InventoryDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var options = configuration.GetSection("Messaging").Get<MessageTransportOptions>() ?? new MessageTransportOptions();
                var host = configuration.GetConnectionString("rabbitmq") ?? "amqp://guest:guest@localhost:5672";

                cfg.Host(new Uri(host));
                cfg.UseMessageRetry(retry => retry.Immediate(options.ImmediateRetryCount));
                cfg.UseDelayedRedelivery(redelivery => redelivery.Intervals(options.RedeliveryDelays.Select(x => TimeSpan.FromSeconds(x)).ToArray()));
                cfg.PrefetchCount = options.PrefetchCount;
                cfg.ConcurrentMessageLimit = options.ConcurrentMessageLimit;

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

public record MessageTransportOptions
{
    public ushort PrefetchCount { get; init; } = 16;

    public int ConcurrentMessageLimit { get; init; } = 8;

    public int ImmediateRetryCount { get; init; } = 3;

    public int[] RedeliveryDelays { get; init; } = new[] { 10, 30, 60 };
}
