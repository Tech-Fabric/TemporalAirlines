using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemporalAirlinesConcept.DAL.Contexts;

namespace TemporalAirlinesConcept.Configuration.ConfigurationExtensions;

public static class MigrationManager
{
    public static WebApplication MigrateDatabase(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        appContext.Database.Migrate();

        return webApp;
    }
}
