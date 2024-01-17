using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.DAL.Factories;

public class UsersFactory
{
    public static List<User> GetUsers()
    {
        var users = new List<User>
            {
                new()
                {
                    Name = "Candidus Fergal",
                    Email = "candidus@fergal.com",
                    Role = "User"
                },
                new()
                {
                    Name = "Avgust Naoise",
                    Email = "avgust@naoise.com",
                    Role = "User"
                },
                new()
                {
                    Name = "Gethin Riderch",
                    Email = "gethin@riderch.com",
                    Role = "User"
                },
                new()
                {
                    Name = "Rabiu Shawkat",
                    Email = "rabiu@shawkat.com",
                    Role = "User"
                }
            };

        return users;
    }
}
