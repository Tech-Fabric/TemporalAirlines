using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Contexts;
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

        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(configuration["DatabaseSettings:ConnectionString"]);
            options.EnableSensitiveDataLogging(false);
        });

        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
        services.Configure<UrlSettings>(configuration.GetSection("UrlSettings"));

        services.AddAutoMapper(typeof(UserProfile));
        services.AddAutoMapper(typeof(TicketProfile));
        services.AddAutoMapper(typeof(FlightProfile));
        services.AddAutoMapper(typeof(SeatProfile));

        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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
            options.LoggerFactory = LoggerFactory.Create(builder =>
                builder.AddTelemetryLogger("Client-T"));
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
