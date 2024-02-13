using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Codec;
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
using Temporalio.Converters;
using Temporalio.Extensions.Hosting;
using Temporalio.Extensions.OpenTelemetry;

namespace TemporalAirlinesConcept.Configuration.ConfigurationExtensions;

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

        services.AddScoped(x => new CosmosClient(configuration["DatabaseSettings:ConnectionString"], new CosmosClientOptions
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
            LimitToEndpoint = true,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
            MaxRetryAttemptsOnRateLimitedRequests = 10
        }));

        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

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

            // Need to check how to get
            options.LoggerFactory = LoggerFactory.Create(builder => builder.AddTelemetryLogger("Client-T"));
            
            options.DataConverter = DataConverter.Default with { PayloadCodec = new EncryptionCodec() };
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
                options.LoggerFactory = LoggerFactory.Create(builder => builder.AddTelemetryLogger("Worker"));
                if (options.ClientOptions != null)
                    options.ClientOptions.DataConverter = DataConverter.Default with { PayloadCodec = new EncryptionCodec() };
            })
            .AddScopedActivities<FlightActivities>()
            .AddWorkflow<FlightWorkflow>()
            .AddScopedActivities<PurchaseActivities>()
            .AddWorkflow<PurchaseWorkflow>()
            .AddScopedActivities<UserRegistrationActivities>()
            .AddWorkflow<UserRegistrationWorkflow>();

        return services;
    }

    public static IServiceCollection ConfigureSession(this IServiceCollection services)
    {
        services.AddSession(options =>
        {
            options.Cookie.Name = ".BookingSession";
            options.IdleTimeout = TimeSpan.FromMinutes(15);
        });

        return services;
    }
}
