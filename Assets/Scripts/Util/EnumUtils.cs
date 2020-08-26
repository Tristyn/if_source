using System;
using System.Collections.Generic;

public static class EnumUtils<T> where T : Enum
{
    public static T[] values;
    public static int valuesLength;
    public static T[] nonZeroValues;
    public static int nonZeroValuesLength;
    public static T last;
    public static string[] names;
    public static string[] nonZeroNames;

    static EnumUtils()
    {
        names = Enum.GetNames(typeof(T));
        Array valueEnums = (T[])Enum.GetValues(typeof(T));
        values = (T[])valueEnums;

        List<string> nonZeroNames1 = new List<string>();
        List<T> nonZeroValues1 = new List<T>();
        int len = values.Length;
        for (int i = 0; i < len; ++i)
        {
            Enum valueEnum = Enum.Parse(typeof(T), values.GetValue(i).ToString()) as Enum;
            int enumValue = Convert.ToInt32(valueEnum);
            if ((byte)enumValue != 0)
            {
                nonZeroNames1.Add(names[i]);
                nonZeroValues1.Add(values[i]);
            }
        }
        nonZeroNames = nonZeroNames1.ToArray();
        nonZeroValues = nonZeroValues1.ToArray();
        nonZeroValuesLength = nonZeroValues.Length;
        valuesLength = len;
        if (len > 0)
        {
            last = values[valuesLength - 1];
        }
    }
}
