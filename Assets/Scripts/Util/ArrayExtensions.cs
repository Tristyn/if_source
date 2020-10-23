using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class ArrayExtensions
{
    public static T[] Append<T>(this T[] array, T element)
    {
        T[] ret = new T[array.Length + 1];
        ret[array.Length] = element;
        for (int i = 0, len = array.Length; i < len; ++i)
        {
            ret[i] = array[i];
        }
        return ret;
    }

    public static T[] Remove<T>(this T[] source, T element)
    {
        int index = Array.IndexOf(source, element);
        if(index == -1)
        {
            return source;
        }
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static T[] Remove<T>(this T[] array, int index)
    {
        T[] ret = new T[array.Length - 1];
        for (int i = 0, len = index; i < len; ++i)
        {
            ret[i] = array[i];
        }
        int retIndex = index;
        for (int i = index + 1, len = array.Length; i < len; ++i, ++retIndex)
        {
            ret[retIndex] = array[i];
        }
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveAtSwapBack<T>(this T[] array, int index, int lastElement)
    {
        array[index] = array[lastElement];
        array[lastElement] = default;
    }

    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this T[] array, T element)
        where T : class
    {
        for (int i = 0, len = array.Length; i < len; ++i)
        {
            if (array[i] == element)
            {
                return true;
            }
        }
        return false;
    }

    public static void ThrowOnNullOrEmpty<T>(this T[] array)
    {
        // seperate throws so the callstack will say which exception
        if (array == null)
        {
            throw new ArgumentNullException();
        }
        if (array.Length == 0)
        {
            throw new ArgumentException();
        }
    }
}
