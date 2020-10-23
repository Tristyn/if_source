using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FastRemoveList<T>
{
    public T[] array;
    public Dictionary<T, int> keys;
    public int size;

    public FastRemoveList()
    {
        array = Array.Empty<T>();
        keys = new Dictionary<T, int>();
    }

    public FastRemoveList(int capacity)
    {
        array = new T[capacity];
        keys = new Dictionary<T, int>(capacity);
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

    public int TryGetIndex(T element)
    {
        if (keys.TryGetValue(element, out int index))
        {
            return index;
        }
        return -1;
    }

    /// <summary>
    /// Removes the element, returns if the element existed in the list.
    /// </summary>
    public bool Remove(T element)
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
            return true;
        }
        return false;
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
}

public static class FastRemoveListExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoFixedUpdate<T>(this FastRemoveList<T> list) where T : IFixedUpdate
    {
        T[] components = list.array;
        for (int i = 0, len = list.size; i < len; ++i)
        {
            components[i].DoFixedUpdate();
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoUpdate<T>(this FastRemoveList<T> list) where T : IUpdate
    {
        T[] components = list.array;
        for (int i = 0, len = list.size; i < len; ++i)
        {
            components[i].DoUpdate();
        }
    }
}