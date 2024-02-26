using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Factories;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.Api.Configuration;

public static class DatabaseInitializer
{
    public static async Task InitializeDb(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userRepository = unitOfWork.Repository<User>();

        var users = UsersFactory.GetUsers();
        var dbUsers = await userRepository.GetAll();

        var usersToAdd = users
            .Where(user => !dbUsers.Any(x => string.Equals(x.Email, user.Email, StringComparison.OrdinalIgnoreCase)));

        foreach (var user in usersToAdd)
        {
            userRepository.Insert(user);
        }

        await unitOfWork.SaveChangesAsync();
    }
}