using TemporalAirlinesConcept.DAL.Factories;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.Api.Configuratoin;

public static class DatabaseInitializer
{
    public static async Task<WebApplication> InitializeDefaultUsers(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var userRepository = unitOfWork.GetUserRepository();

        var users = UsersFactory.GetUsers();
        var dbUsers = await userRepository.GetUsersAsync();

        foreach (var user in users)
        {
            if (!dbUsers.Any(x => string.Equals(x.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                await userRepository.AddUserAsync(user);
            }
        }

        return webApp;
    }
}