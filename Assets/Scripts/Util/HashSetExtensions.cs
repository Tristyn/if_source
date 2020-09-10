using System;
using System.Collections.Generic;
using UnityEngine;
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

    public static T[] ToArray<T>(this HashSet<T> set)
    {
        int count = set.Count;
        if (count == 0)
        {
            return Array.Empty<T>();
        }

        int i = 0;
        T[] ret = new T[count];
        foreach (T element in set)
        {
            ret[i] = element;
            ++i;
        }
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
