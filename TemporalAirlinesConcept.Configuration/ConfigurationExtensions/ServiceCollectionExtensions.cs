using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Implementations;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Implementations.Purchase;
using TemporalAirlinesConcept.Services.Implementations.User;
using TemporalAirlinesConcept.Services.Implementations.UserRegistration;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Profiles;
using Temporalio.Extensions.Hosting;
using Temporalio.Extensions.OpenTelemetry;

namespace TemporalAirlinesConcept.Configuration.ConfiguratoinExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, ConsoleExporterOutputTargets targets)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddConsoleExporter(o => o.Targets = targets);
                builder.AddSource(
                    TracingInterceptor.ClientSource.Name,
                    TracingInterceptor.WorkflowsSource.Name,
                    TracingInterceptor.ActivitiesSource.Name);
            });

        services.AddScoped(x => new CosmosClient(configuration["DatabaseSettigns:ConnectionString"], new CosmosClientOptions
        {
            HttpClientFactory = () =>
            {
                var httpMessageHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return new HttpClient(httpMessageHandler);
            },
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true
        }));

        services.Configure<DatabaseSettigns>(configuration.GetSection("DatabaseSettigns"));

        services.AddAutoMapper(typeof(UserProfile));
        services.AddAutoMapper(typeof(FlightProfile));

        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRegistrationService, UserRegistrationService>();

        return services;
    }

    public static IServiceCollection ConfigureTemporalClient(this IServiceCollection services)
    {
        services.AddTemporalClient(options =>
        {
            options.TargetHost = Temporal.DefaultHost;
            options.Interceptors = [new TracingInterceptor()];
        });

        return services;
    }

    public static IServiceCollection ConfigureTemporalWorker(this IServiceCollection services)
    {
        services
            .AddHostedTemporalWorker(
                clientTargetHost: Temporal.DefaultHost,
                clientNamespace: Temporal.DefaultNamespace,
                taskQueue: Temporal.DefaultQueue)
            .ConfigureOptions(options =>
            {
                options.Interceptors = [new TracingInterceptor()];
            })
            .AddScopedActivities<FlightActivities>()
            .AddWorkflow<FlightWorkflow>()
            .AddScopedActivities<PurchaseActivities>()
            .AddWorkflow<PurchaseWorkflow>()
            .AddScopedActivities<UserRegistrationActivities>()
            .AddWorkflow<UserRegistrationWorkflow>();

        return services;
    }
}
