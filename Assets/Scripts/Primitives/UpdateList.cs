using System.Runtime.CompilerServices;

public interface IFixedUpdate
{
    void DoFixedUpdate();
}

public interface IUpdate
{
    void DoUpdate();
}

public sealed class UpdateList<T>
{
    public FastRemoveList<T> list;
    public FastRemoveList<T> remove = new FastRemoveList<T>(4);

    public UpdateList()
    {
        list = new FastRemoveList<T>();
    }

    public UpdateList(int capacity)
    {
        list = new FastRemoveList<T>(capacity);
    }


    /// <summary>
    /// Adds the element to the list. Returns false if the element already existed and wasn't added.
    /// </summary>
    public bool Add(T element)
    {
        remove.Remove(element);
        return list.Add(element);
    }

    public bool Contains(T element)
    {
        return list.Contains(element) && !remove.Contains(element);
    }

    public int TryGetIndex(T element)
    {
        int index = list.TryGetIndex(element);
        if (index != -1 && remove.Contains(element))
        {
            return -1;
        }
        return index;
    }

    /// <summary>
    /// Removes the element at the end of the update.
    /// </summary>
    public void Remove(T element)
    {
        if (list.Contains(element))
        {
            remove.Add(element);
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

    public void RemoveImmediate()
    {
        T[] removeArray = remove.array;
        for (int i = 0, len = remove.size; i < len; ++i)
        {
            list.Remove(removeArray[i]);
        }
        remove.Clear();
    }
}
public static class UpdateListExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoFixedUpdate<T>(this UpdateList<T> list) where T : IFixedUpdate
    {
        list.list.DoFixedUpdate();
        list.RemoveImmediate();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoUpdate<T>(this UpdateList<T> list) where T : IUpdate
    {
        list.list.DoUpdate();
        list.RemoveImmediate();
    }
}