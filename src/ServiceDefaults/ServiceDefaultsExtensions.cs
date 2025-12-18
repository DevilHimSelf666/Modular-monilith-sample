using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static void AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddServiceDiscovery();
            http.AddStandardResilienceHandler();
        });

        builder.Services.AddHealthChecks();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddOtlpExporterIfConfigured(builder.Configuration);
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddMeter(builder.Environment.ApplicationName);
                metrics.AddOtlpExporterIfConfigured(builder.Configuration);
            });
    }

    public static void UseServiceDefaults(this WebApplication app)
    {
        app.MapHealthChecks("/health");
    }

    private static TracerProviderBuilder AddOtlpExporterIfConfigured(this TracerProviderBuilder builder, IConfiguration configuration)
    {
        var endpoint = configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
        return string.IsNullOrWhiteSpace(endpoint)
            ? builder
            : builder.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
    }

    private static MeterProviderBuilder AddOtlpExporterIfConfigured(this MeterProviderBuilder builder, IConfiguration configuration)
    {
        var endpoint = configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
        return string.IsNullOrWhiteSpace(endpoint)
            ? builder
            : builder.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
    }
}
