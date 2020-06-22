using System;
using System.Collections.Generic;

public static class EnumUtils<T> where T : Enum
{
    public static T[] values;
    public static T[] nonZeroValues;
    public static string[] names;
    public static string[] nonZeroNames;

    static EnumUtils()
    {
        names = Enum.GetNames(typeof(T));
        Array valueEnums = (T[])Enum.GetValues(typeof(T));
        values = (T[])valueEnums;

        List<string> nonZeroNames1 = new List<string>();
        List<T> nonZeroValues1 = new List<T>();
        for (int i = 0, len = valueEnums.Length; i < len; i++)
        {
            if ((int)valueEnums.GetValue(i) != 0)
            {
                nonZeroNames1.Add(names[i]);
                nonZeroValues1.Add(values[i]);
            }
        }
        nonZeroNames = nonZeroNames1.ToArray();
        nonZeroValues = nonZeroValues1.ToArray();
    }
}
