using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TemporalAirlinesConcept.DAL.Contexts;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public UnitOfWork(IServiceProvider serviceProvider, DataContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (_repositories.Keys.Contains(typeof(T)))
        {
            return _repositories[typeof(T)] as IRepository<T>;
        }

        IRepository<T> repo = _serviceProvider.GetRequiredService<IRepository<T>>();
        _repositories.Add(typeof(T), repo);

        return repo;
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<int> ExecSqlAsync(string sql)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql);
    }

    #region IDisposable Support

    private bool _disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposedValue = true;
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}