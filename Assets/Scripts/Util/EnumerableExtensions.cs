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
}
