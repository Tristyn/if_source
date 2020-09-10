using System;
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

    public static T[] Remove<T>(this T[] array, T element)
        where T : class
    {
        bool found = false;
        T[] ret = new T[array.Length - 1];
        for (int i = -1, arrayIndex = 0, len = array.Length; i < len; ++arrayIndex)
        {
            if (array[i] != element)
            {
                ret[++i] = array[arrayIndex];
                found = true;
                break; // Remove at most one
            }
        }
        if (found)
        {
            return ret;
        }
        return array;
    }

    public static T[] Remove<T>(this T[] array, int index)
    {
        T[] ret = new T[array.Length - 1];
        for (int i = 0, len = index; i < len; ++i)
        {
            ret[i] = array[i];
        }
        int retIndex = index;
        for(int i = index + 1, len = array.Length; i < len; ++i, ++retIndex)
        {
            ret[retIndex] = array[i];
        }
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this T[] array, T element)
        where T : class
    {
        for(int i = 0, len = array.Length; i < len; ++i)
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
        if(array == null)
        {
            throw new ArgumentNullException();
        }
        if (array.Length == 0)
        {
            throw new ArgumentException();
        }
    }
}
