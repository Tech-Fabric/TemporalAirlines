using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

namespace TemporalAirlinesConcept.Configuration.ConfigurationExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
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
            .AddScopedActivities<FlightActivities>()
            .AddWorkflow<FlightWorkflow>()
            .AddScopedActivities<PurchaseActivities>()
            .AddWorkflow<PurchaseWorkflow>()
            .AddScopedActivities<UserRegistrationActivities>()
            .AddWorkflow<UserRegistrationWorkflow>();

        return services;
    }
}
