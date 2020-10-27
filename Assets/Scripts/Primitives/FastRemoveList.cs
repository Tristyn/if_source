using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

public struct FastRemoveList<T>
{
    public T[] array;
    public Dictionary<T, int> keys;
    public int size;

    public static string entityUpdate = typeof(T).Name + " Update";
    public static string entitiesUpdate = typeof(T).Name + " Update List";
    public static string entityFixedUpdate= typeof(T).Name + " Fixed Update";
    public static string entitiesFixedUpdate = typeof(T).Name + " Fixed Update List";

    public FastRemoveList(int capacity)
    {
        array = new T[capacity];
        keys = new Dictionary<T, int>(capacity);
        size = 0;
    }

    /// <summary>
    /// Adds the element to the list. Returns false if the element already existed and wasn't added.
    /// </summary>
    public bool Add(T element)
    {
        if (!keys.ContainsKey(element))
        {
            if (size == array.Length)
            {
                T[] expanded = new T[Mathf.Max(array.Length * 2, 4)];
                Array.Copy(array, expanded, array.Length);
                array = expanded;
            }

            array[size] = element;
            keys.Add(element, size);
            ++size;
            return true;
        }
        return false;
    }

    public bool Contains(T element)
    {
        return keys.ContainsKey(element);
    }

    public int TryGetIndex(T element)
    {
        if (keys.TryGetValue(element, out int index))
        {
            return index;
        }
        return -1;
    }

    /// <summary>
    /// Removes the element.
    /// </summary>
    public void Remove(T element)
    {
        if (keys.TryGetValue(element, out int index))
        {
            size--;
            keys.Remove(element);
            if (index != size)
            {
                T lastElement = array[size];
                array[index] = lastElement;
                keys[lastElement] = index;
            }
            array[size] = default;
        }
    }

    // Aggressive inlining to move the branch prediction up the stack
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAdded(bool added, T element)
    {
        if (added)
        {
            Add(element);
        }
        else
        {
            Remove(element);
        }
    }

    public void Clear()
    {
        for(int i = 0, len = size; i < len; ++i)
        {
            array[i] = default;
        }
        keys.Clear();
        size = 0;
    }
}

public static class FastRemoveListExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoFixedUpdate<T>(this FastRemoveList<T> list) where T : IFixedUpdate
    {
        Profiler.BeginSample(FastRemoveList<T>.entitiesFixedUpdate);
        T[] components = list.array;
        for (int i = 0, len = list.size; i < len; ++i)
        {
            Profiler.BeginSample(FastRemoveList<T>.entityFixedUpdate);
            components[i].DoFixedUpdate();
            Profiler.EndSample();
        }
        Profiler.EndSample();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoUpdate<T>(this FastRemoveList<T> list) where T : IUpdate
    {
        Profiler.BeginSample(FastRemoveList<T>.entitiesUpdate);
        T[] components = list.array;
        for (int i = 0, len = list.size; i < len; ++i)
        {
            Profiler.BeginSample(FastRemoveList<T>.entityUpdate);
            components[i].DoUpdate();
            Profiler.EndSample();
        }
        Profiler.EndSample();
    }
}