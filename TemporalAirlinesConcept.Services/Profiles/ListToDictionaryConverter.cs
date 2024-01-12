using AutoMapper;

namespace TemporalAirlinesConcept.Services.Profiles;

public class ListToDictionaryConverter : ITypeConverter<List<string>, Dictionary<string, string?>>
{
    public Dictionary<string, string?> Convert(List<string> source,
        Dictionary<string, string?> destination, ResolutionContext context)
    {
        var dictionary = source.ToDictionary(x => x, x => (string)null);

        return dictionary;
    }
}
