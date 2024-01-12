using System.Linq.Expressions;

namespace TemporalAirlinesConcept.DAL.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class
    {
        IQueryable<T> Table { get; }

        Task<IList<T>> GetAll();

        IQueryable<T> Get(Expression<Func<T, bool>> predicate);

        T Find(Expression<Func<T, bool>> predicate);

        Task<T> FindAsync(Expression<Func<T, bool>> predicate);

        bool Any(Expression<Func<T, bool>> predicate);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        bool Any();

        Task<bool> AnyAsync();

        T Insert(T entity);

        void InsertRange(IEnumerable<T> entities);

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

        IQueryable<T> FromSql(string sql, params object[] parameters);
    }
}
