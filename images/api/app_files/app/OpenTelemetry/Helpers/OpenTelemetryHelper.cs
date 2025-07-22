using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace Inventory.OpenTelemetry.Helpers
{
    public class OpenTelemetryHelper
    {
        internal static void ConfigureOpenTelemetry(WebApplicationBuilder builder, IConfiguration configuration)
        {
            var otlpHost = configuration["Otlp:EndpointHost"] ?? throw new ArgumentNullException(nameof(configuration), "INVALID_OTLP_ENDPOINTHOST");
            var otlpPort = configuration.GetValue<int?>("Otlp:EndpointPort") ?? throw new ArgumentNullException(nameof(configuration), "INVALID_OTLP_ENDPOINTPORT");
            var otlpPrefix = configuration["Otlp:Prefix"] ?? throw new ArgumentNullException(nameof(configuration), "INVALID_OTLP_PREFIX");

            builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(otlpPrefix))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    //.AddConsoleExporter() // For local debugging; replace or add OTLP exporter for production
                    .AddOtlpExporter(options => 
                        { 
                            options.Endpoint = new Uri($"http://{otlpHost}:{otlpPort}");
                        }
                    );
            });

            builder.Logging.AddOpenTelemetry(options =>
            {
                // You can add an OTLP exporter here if you want logs via OTLP, but for Loki+Promtail, just enrich logs
                options.IncludeScopes = true; // This is important for trace/span context
                options.ParseStateValues = true; // This is important for trace/span context
                options.IncludeFormattedMessage = true; // This is important for trace/span context
            });
        }
    }
}
