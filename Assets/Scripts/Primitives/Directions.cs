using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public enum Directions : byte
{
    North = 0, East = 1, South = 2, West = 3
}

public static class DirectionsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DirectionsFlag ToFlag(this Directions direction)
    {
        return (DirectionsFlag)(1 << (int)direction);
    }

    public static Vector3 ToOffset(this Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return new Vector3(1, 0, 0);
            case Directions.East:
                return new Vector3(0, 0, -1);
            case Directions.South:
                return new Vector3(-1, 0, 0);
            case Directions.West:
                return new Vector3(0, 0, 1);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public static Vector3 ToOffset(this Directions direction, float scale)
    {
        switch (direction)
        {
            case Directions.North:
                return new Vector3(scale, 0, 0);
            case Directions.East:
                return new Vector3(0, 0, -scale);
            case Directions.South:
                return new Vector3(-scale, 0, 0);
            case Directions.West:
                return new Vector3(0, 0, scale);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static Vector3Int ToOffsetInt(this Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return new Vector3Int(1, 0, 0);
            case Directions.East:
                return new Vector3Int(0, 0, -1);
            case Directions.South:
                return new Vector3Int(-1, 0, 0);
            case Directions.West:
                return new Vector3Int(0, 0, 1);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static Vector3Int ToOffsetInt(this Directions direction, int scale)
    {
        switch (direction)
        {
            case Directions.North:
                return new Vector3Int(scale, 0, 0);
            case Directions.East:
                return new Vector3Int(0, 0, -scale);
            case Directions.South:
                return new Vector3Int(-scale, 0, 0);
            case Directions.West:
                return new Vector3Int(0, 0, scale);
            default:
                throw new ArgumentOutOfRangeException("Value: " + (int)direction);
        }
    }

    public static bool IsNeighbor(this Vector3Int from, Vector3Int to)
    {
        Vector3Int offset = to - from;
        if (offset.y == 0)
        {
            if (offset.x == 0)
            {
                if (offset.z == -1)
                {
                    return true;
                }
                else if (offset.z == 1)
                {
                    return true;
                }
            }
            else if (offset.z == 0)
            {
                if (offset.x == 1)
                {
                    return true;
                }
                else if (offset.x == -1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static (bool isNeighbor, Directions direction) ToDirection(this Vector3Int from, Vector3Int to)
    {
        Vector3Int offset = to - from;
        if (offset.y == 0)
        {
            if (offset.x == 0)
            {
                if (offset.z == -1)
                {
                    return (true, Directions.East);
                }
                else if (offset.z == 1)
                {
                    return (true, Directions.West);
                }
            }
            else if (offset.z == 0)
            {
                if (offset.x == 1)
                {
                    return (true, Directions.North);
                }
                else if (offset.x == -1)
                {
                    return (true, Directions.South);
                }
            }
        }
        return (false, Directions.North);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Directions Left(this Directions direction)
    {
        int result = ((byte)direction - 1) & 0b11;
        return (Directions)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Directions Right(this Directions direction)
    {
        int result = ((byte)direction + 1) & 0b11;
        return (Directions)result;
    }

    public static Directions Inverse(this Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return Directions.South;
            case Directions.East:
                return Directions.West;
            case Directions.South:
                return Directions.North;
            case Directions.West:
                return Directions.East;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}