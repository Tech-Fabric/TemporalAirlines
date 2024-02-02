using Microsoft.Azure.Cosmos;
using TemporalAirlinesConcept.DAL.Factories;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.Api.Configuration;

public static class DatabaseInitializer
{
    public static async Task InitializeDb(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userRepository = unitOfWork.GetUserRepository();

        var users = UsersFactory.GetUsers();
        var dbUsers = await userRepository.GetUsersAsync();

        var usersToAdd = users
            .Where(user => !dbUsers.Any(x => string.Equals(x.Email, user.Email, StringComparison.OrdinalIgnoreCase)));

        foreach (var user in usersToAdd)
        {
            await userRepository.AddUserAsync(user);
        }
    }

    public static async Task CheckCosmosDb(this WebApplication webApp, Action<CheckDbOptions> configure = null)
    {
        CheckDbOptions checkDbOptions = GetOptions(configure);

        using var scope = webApp.Services.CreateScope();

        var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var errorMessage = "Error during Cosmos DB initialization";
        byte attemtCount = 0;
        double interval = checkDbOptions.Interval;

        do
        {
            try
            {
                await cosmosClient.ReadAccountAsync();

                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"{errorMessage}: {ex.InnerException?.Message ?? ex.Message}");

                if (attemtCount == checkDbOptions.MaxAttemtCount)
                    throw;
            }

            attemtCount++;

            await Task.Delay(TimeSpan.FromSeconds(interval));

            interval *= checkDbOptions.Multiplier;

        } while (attemtCount <= checkDbOptions.MaxAttemtCount);

        throw new Exception(errorMessage);
    }

    private static CheckDbOptions GetOptions(Action<CheckDbOptions> configure)
    {
        CheckDbOptions checkDbOptions = new();

        if (configure != null)
        {
            configure(checkDbOptions);
        }

        return checkDbOptions;
    }
}