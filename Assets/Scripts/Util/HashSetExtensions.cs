using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public static class HashSetExtensions
{
    public static void AddList<T>(this HashSet<T> set, List<T> list)
    {
        for(int i = 0, len = list.Count; i < len; ++i)
        {
            set.Add(list[i]);
        }
    }
    public static void AddArray<T>(this HashSet<T> set, T[] array)
    {
        for (int i = 0, len = array.Length; i < len; ++i)
        {
            set.Add(array[i]);
        }
    }

    public static T[] ToArray<T>(this HashSet<T> set)
    {
        int count = set.Count;
        if (count == 0)
        {
            return Array.Empty<T>();
        }

        T[] ret = new T[count];
        set.CopyTo(ret);
        return ret;
    }

    public static List<T> ToList<T>(this HashSet<T> set)
    {
        List<T> list = ListPool<T>.Get();
        list.EnsureCapacity(set.Count);
        foreach (T element in set)
        {
            list.Add(element);
            // List.Add(IEnumerable) may be faster in terms of cycles but this is boxing/garbage free
        }
        return list;
    }
}
