namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;

    int SaveChanges();

    Task<int> SaveChangesAsync();

    Task<int> ExecSqlAsync(string sql);
}