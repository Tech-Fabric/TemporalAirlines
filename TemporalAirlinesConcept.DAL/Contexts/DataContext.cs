using Microsoft.EntityFrameworkCore;
using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.DAL.Contexts;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
