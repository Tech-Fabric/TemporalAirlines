using Microsoft.Extensions.DependencyInjection;

namespace TemporalAirlinesConcept.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            //services.AddDbContext<>(options =>
            //{
            //});

            //services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //services.AddScoped<IUnitOfWork, UnitOfWork>();

            //services.AddScoped<IUsersService, UsersService>();
            //services.AddScoped<IPurchasesService, PurchasesService>();

            //services.AddScoped<IBookingService, BookingService>();
            //services.AddScoped<IUserPurchaseService, UserPurchaseService>();

            //services.AddScoped<IVersioningService, VersioningService>();

            return services;
        }

        public static IServiceCollection ConfigureTemporalClient(this IServiceCollection services)
        {
            services.AddTemporalClient(options =>
            {
                options.TargetHost = "localhost:7233";
            });

            return services;
        }

        public static IServiceCollection ConfigureTemporalWorker(this IServiceCollection services)
        {
            services
                .AddHostedTemporalWorker(
                    clientTargetHost: "localhost:7233",
                    clientNamespace: "default",
                    taskQueue: "my-task-queue")
                // Add the activities class at the scoped level
                //.AddScopedActivities<UserActivities>()
                //.AddScopedActivities<PurchaseActivities>()
                //.AddWorkflow<UserPurchseWorkflow>()
                //.AddWorkflow<BookingWorkflow>()
                //.AddWorkflow<ChildBookingWorkflow>()
                ;

            // Version 1
            //services
            //    .AddHostedTemporalWorker(
            //        clientTargetHost: "localhost:7233",
            //        clientNamespace: "default",
            //        taskQueue: "versioning-queue")
            //    .AddScopedActivities<VersioningActivities>()
            //    .AddWorkflow<VersioningWorkflowV1>();

            return services;
        }
    }
}
