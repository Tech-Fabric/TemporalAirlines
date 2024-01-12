namespace TemporalAirlinesConcept.DAL.Interfaces
{
    public interface IEntity<TKey> where TKey : IComparable<TKey>, IComparable
    {
        TKey Id { get; set; }
    }
}
