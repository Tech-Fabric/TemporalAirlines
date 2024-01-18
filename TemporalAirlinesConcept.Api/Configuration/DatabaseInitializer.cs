using TemporalAirlinesConcept.DAL.Factories;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.Api.Configuration;

public static class DatabaseInitializer
{
    public static async Task InitializeDefaultUsers(this WebApplication webApp)
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
}