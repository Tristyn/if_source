using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.Except(new[] { element });
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T element, IEqualityComparer<T> comparer)
    {
        return enumerable.Except(new[] { element }, comparer);
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
    {
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        foreach (var entry in keyValuePairs)
        {
            dictionary.Add(entry.Key, entry.Value);
        }
        return dictionary;
    }
}
