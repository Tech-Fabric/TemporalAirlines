using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace TemporalAirlinesConcept.Configuration.ConfigurationExtensions
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder AddTelemetryLogger(this ILoggingBuilder builder,
            string serviceName,
            string version = "1.0.0",
            LogLevel minimumLogLevel = LogLevel.Information,
            ConsoleExporterOutputTargets outputTargets = ConsoleExporterOutputTargets.Debug)
        {
            builder.AddOpenTelemetry(logging =>
            {
                logging.IncludeScopes = true;

                logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: version));

                logging.AddConsoleExporter(o => o.Targets = outputTargets);
            })
            .SetMinimumLevel(minimumLogLevel);

            return builder;
        }
    }
}
