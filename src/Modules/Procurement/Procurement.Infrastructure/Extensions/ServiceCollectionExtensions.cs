using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Procurement.Application;
using Procurement.Application.Abstractions;
using Procurement.Infrastructure.Data;
using Procurement.Infrastructure.Services;

namespace Procurement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcurementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BudgetOptions>(configuration.GetSection("Budget"));
        services.Configure<MessageTransportOptions>(configuration.GetSection("Messaging"));

        services.AddDbContext<ProcurementDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ProcurementDb") ?? throw new InvalidOperationException("Connection string 'ProcurementDb' not found.");
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", ProcurementDbContext.Schema));
        });

        services.AddScoped<IProcurementRequestRepository, ProcurementRequestRepository>();
        services.AddScoped<IBudgetAvailabilityChecker, BudgetAvailabilityChecker>();
        services.AddScoped<IWorkflowStarter, WorkflowStarter>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddEntityFrameworkOutbox<ProcurementDbContext>(outbox =>
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
