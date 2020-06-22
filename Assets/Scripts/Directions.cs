using UnityEngine;
using System;

public enum Directions
{
    None, North, East, South, West
}

public static class DirectionsExtensions
{
    public static Vector3Int ToOffset(this Directions direction)
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

    public static Directions ToDirection(this Vector3Int offset)
    {
        if (offset == new Vector3Int(1, 0, 0))
        {
            return Directions.North;
        }
        else if (offset == new Vector3Int(0, 0, -1))
        {
            return Directions.East;
        }
        else if (offset == new Vector3Int(-1, 0, 0))
        {
            return Directions.South;
        }
        else if (offset == new Vector3Int(0, 0, 1))
        {
            return Directions.West;
        }
        return Directions.None;
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