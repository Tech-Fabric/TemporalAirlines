using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TemporalAirlinesConcept.DAL.Contexts;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DataContext _context = null;
    private DbSet<T> _entities;

    public Repository(DataContext context)
    {
        _context = context;
    }

    private DbSet<T> Entities
    {
        get
        {
            _entities ??= _context.Set<T>();

            return _entities;
        }
    }

    private void ThrowIfEntityIsNull(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
    }

    public virtual IQueryable<T> Table
    {
        get
        {
            return Entities;
        }
    }

    public async Task<IList<T>> GetAll()
    {
        return await Entities.ToListAsync();
    }

    public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
    {
        return Entities.Where(predicate);
    }

    public T Find(Expression<Func<T, bool>> predicate)
    {
        return Entities.FirstOrDefault(predicate);
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await Entities.FirstOrDefaultAsync(predicate);
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        return Entities.Any(predicate);
    }

    public bool Any()
    {
        return Entities.Any();
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await Entities.AnyAsync(predicate);
    }

    public async Task<bool> AnyAsync()
    {
        return await Entities.AnyAsync();
    }

    public T Insert(T entity)
    {
        ThrowIfEntityIsNull(entity);
        var addedEntity = Entities.Add(entity);

        return addedEntity.Entity;
    }

    public void InsertRange(IEnumerable<T> entities)
    {
        Entities.AddRange(entities);
    }

    public void Update(T entity)
    {
        ThrowIfEntityIsNull(entity);
        _context.Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _context.UpdateRange(entities);
    }

    public void Remove(T entity)
    {
        ThrowIfEntityIsNull(entity);
        Entities.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        Entities.RemoveRange(entities);
    }

    public IQueryable<T> FromSql(string sql, params object[] parameters)
    {
        return Entities.FromSqlRaw(sql, parameters);
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
                _entities = null;
            }

            _disposedValue = true;
        }
    }

    ~Repository()
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