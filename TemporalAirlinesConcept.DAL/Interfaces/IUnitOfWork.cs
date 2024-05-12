using Microsoft.EntityFrameworkCore.Storage;

namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;

    int SaveChanges();

    Task<int> SaveChangesAsync();

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> ExecSqlAsync(string sql);
}