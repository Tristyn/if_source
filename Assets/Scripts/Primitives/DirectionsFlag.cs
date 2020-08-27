using System;
using System.Runtime.CompilerServices;

[Flags]
public enum DirectionsFlag : byte
{
    North = 1, East = 2, South = 4, West = 8
}

public static class DirectionsFlagExtensions
{
    // This could be a generic flag utils class

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get(this DirectionsFlag a, DirectionsFlag b)
    {
        return (a & b) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get(this DirectionsFlag a, Directions b)
    {
        return (a & b.ToFlag()) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any(this DirectionsFlag a)
    {
        return ((byte)a) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTrue(this ref DirectionsFlag a, DirectionsFlag b)
    {
        a |= b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTrue(this ref DirectionsFlag a, Directions b)
    {
        a |= b.ToFlag();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFalse(this ref DirectionsFlag a, DirectionsFlag b)
    {
        a &= ~b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFalse(this ref DirectionsFlag a, Directions b)
    {
        a &= ~b.ToFlag();
    }
}
