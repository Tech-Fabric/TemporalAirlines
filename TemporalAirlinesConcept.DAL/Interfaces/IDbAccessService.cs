namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IDbAccessService<T> where T : class
{
    Task<T> GetItemAsync(string id);

    Task<IEnumerable<T>> GetItemsAsync(string query);

    Task AddItemAsync(T item, string id);

    Task UpdateItemAsync(string id, T item);

    Task DeleteItemAsync(string id);
}