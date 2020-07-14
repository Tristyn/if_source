using Boo.Lang;

public static class ListExtensions
{
    public static void RemoveAtSwapBack<T>(this List<T> list, int index)
    {
        int lastElement = list.Count - 1;
        list[index] = list[lastElement];
        list.RemoveAt(lastElement);
    }
}
