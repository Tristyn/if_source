using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ListExtensions
{
    public static void RemoveAtSwapBack<T>(this List<T> list, int index)
    {
        int lastElement = list.Count - 1;
        list[index] = list[lastElement];
        list.RemoveAt(lastElement);
    }

    public static void AddList<T>(this List<T> list, List<T> range)
    {
        for (int i = 0, len = range.Count; i < len; ++i)
        {
            list.Add(range[i]);
        }
    }

    public static void AddArray<T>(this List<T> list, T[] range)
    {
        for (int i = 0, len = range.Length; i < len; ++i)
        {
            list.Add(range[i]);
        }
    }

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Resizes the lists internal array to capacity only if it is less.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureCapacity<T>(this List<T> list, int capacity)
    {
        if (list.Capacity < capacity)
        {
            list.Capacity = capacity; // resize
        }
    }
}
