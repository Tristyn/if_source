using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Bounds3Int
{
    public Vector3Int min;
    public Vector3Int max;

    public Bounds3Int(Vector3Int min, Vector3Int max)
    {
        this.min = min;
        this.max = max;
    }

    public Vector3 center => min + ((Vector3)(max - min) * 0.5f) + new Vector3(0.5f, 0.5f, 0.5f);

    public Vector3 bottomCenter
    {
        get
        {
            Vector3 center = this.center;
            center.y = min.y;
            return center;
        }
    }

    public Vector3 topCenter
    {
        get
        {
            Vector3 center = this.center;
            center.y = max.y + 1f;
            return center;
        }
    }

    public Vector3Int size => max - min + Vector3Int.one;

    public bool Contains(Vector3Int position)
    {
        return position.x >= min.x && position.y >= min.y && position.z >= min.z
            && position.x <= max.x && position.y <= max.y && position.z <= max.z;
    }

    public bool Overlaps(Bounds3Int b)
    {
        return max.x >= b.min.x && b.max.x >= min.x
            && max.y >= b.min.y && b.max.y >= min.y
            && max.z >= b.min.z && b.max.z >= min.z;
    }

    public bool Perimeter(Vector3Int vector)
    {
        if (vector.x == min.x || vector.x == max.x)
        {
            if (vector.y == min.y || vector.y == max.y)
            {
                if (vector.z == min.z || vector.z == max.z)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerable<(Vector3Int outerTile, Vector3Int innerTile)> EnumeratePerimeter()
    {
        for (int x = min.x; x <= max.x; ++x)
        {
            yield return (new Vector3Int(x, min.y, min.z - 1), new Vector3Int(x, min.y, min.z));
            yield return (new Vector3Int(x, min.y, max.z + 1), new Vector3Int(x, min.y, max.z));
        }
        for (int z = min.z; z <= max.z; ++z)
        {
            yield return (new Vector3Int(min.x - 1, min.y, z), new Vector3Int(min.x, min.y, z));
            yield return (new Vector3Int(max.x + 1, min.y, z), new Vector3Int(max.x, min.y, z));
        }
    }

    public IEnumerable<(Vector3Int outerTile, Vector3Int innerTile, Directions direction)> EnumeratePerimeterClockwise()
    {
        for (int z = min.z; z <= max.z; ++z)
        {
            yield return (new Vector3Int(max.x + 1, min.y, z), new Vector3Int(max.x, min.y, z), Directions.North);
        }
        for (int x = min.x; x <= max.x; ++x)
        {
            yield return (new Vector3Int(x, min.y, min.z - 1), new Vector3Int(x, min.y, min.z), Directions.East);
        }
        for (int z = min.z; z <= max.z; ++z)
        {
            yield return (new Vector3Int(min.x - 1, min.y, z), new Vector3Int(min.x, min.y, z), Directions.South);
        }
        for (int x = min.x; x <= max.x; ++x)
        {
            yield return (new Vector3Int(x, min.y, max.z + 1), new Vector3Int(x, min.y, max.z), Directions.West);
        }
    }

    public static bool operator ==(Bounds3Int a, Bounds3Int b)
    {
        return a.min == b.min && a.max == b.max;
    }

    public static bool operator !=(Bounds3Int a, Bounds3Int b)
    {
        return a.min != b.min || a.max != b.max;
    }

    public override bool Equals(object obj)
    {
        if (obj is Bounds3Int)
        {
            var bounds = (Bounds3Int)obj;
            return this == bounds;

        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = (hash * 7) + min.GetHashCode();
            hash = (hash * 7) + max.GetHashCode();
            return hash;
        }
    }
}
