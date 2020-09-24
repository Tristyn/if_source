using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public Bounds3Int(int minx, int miny, int minz, int maxx, int maxy, int maxz)
    {
        min = new Vector3Int(minx, miny, minz);
        max = new Vector3Int(maxx, maxy, maxz);
    }

    public static Bounds3Int Create(Vector3Int min, Vector3Int size)
    {
        Vector3Int max = min + size - new Vector3Int(1, 1, 1);
        return new Bounds3Int(min, max);
    }

    public static Bounds3Int FromPoints(Vector3Int a, Vector3Int b)
    {
        return new Bounds3Int(
            new Vector3Int(
                Mathf.Min(a.x, b.x),
                Mathf.Min(a.y, b.y),
                Mathf.Min(a.z, b.z)),
            new Vector3Int(
                Mathf.Max(a.x, b.x),
                Mathf.Max(a.y, b.y),
                Mathf.Max(a.z, b.z)));
    }

    /// <summary>
    /// Returns a bounding box that encompasses all bounds
    /// </summary>
    public static Bounds3Int BoundingBox(Bounds3Int[] bounds)
    {
        if (bounds.Length == 0)
        {
            return new Bounds3Int(0, 0, 0, 0, 0, 0);
        }
        Bounds3Int boundingBox = bounds[0];
        for (int i = 1, len = bounds.Length; i < len; ++i)
        {
            boundingBox.min = Vector3Int.Min(boundingBox.min, bounds[i].min);
            boundingBox.max = Vector3Int.Max(boundingBox.max, bounds[i].max);
        }
        return boundingBox;
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

    public int volume
    {
        get
        {
            Vector3Int size = this.size;
            return size.x * size.y * size.z;
        }
    }

    public int area
    {
        get
        {
            Vector3Int size = this.size;
            return size.x * size.z;
        }
    }

    public bool Contains(Vector3Int position)
    {
        return position.x >= min.x && position.y >= min.y && position.z >= min.z
            && position.x <= max.x && position.y <= max.y && position.z <= max.z;
    }

    public static bool Contains(Bounds3Int[] container, Bounds3Int bounds)
    {
        Vector3Int position = new Vector3Int(bounds.min.x, bounds.min.y, bounds.min.z);
        for (position.y = bounds.min.y; position.y <= bounds.max.y; ++position.y)
        {
            for (position.x = bounds.min.x; position.x <= bounds.max.x; ++position.x)
            {
                for (position.z = bounds.min.z; position.z <= bounds.max.z; ++position.z)
                {
                    if(!Contains(container, position))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public static bool Contains(Bounds3Int[] container, Vector3Int position)
    {
        for (int i = 0, len = container.Length; i < len; ++i)
        {
            if (container[i].Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public bool Overlaps(Bounds3Int b)
    {
        return max.x >= b.min.x && b.max.x >= min.x
            && max.y >= b.min.y && b.max.y >= min.y
            && max.z >= b.min.z && b.max.z >= min.z;
    }

    public bool Overlaps(Bounds3Int[] b)
    {
        for (int i = 0, len = b.Length; i < len; ++i)
        {
            if (Overlaps(b[i]))
            {
                return true;
            }
        }
        return false;
    }

    public bool Overlaps(List<Bounds3Int> b)
    {
        for (int i = 0, len = b.Count; i < len; ++i)
        {
            if (Overlaps(b[i]))
            {
                return true;
            }
        }
        return false;
    }

    // Gets the minimum vector
    public Vector3Int GetMin(Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return new Vector3Int(max.x, min.y, min.z);
            case Directions.East:
                return min;
            case Directions.South:
                return min;
            case Directions.West:
                return new Vector3Int(min.x, min.y, max.z);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Vector3Int GetMax(Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return max;
            case Directions.East:
                return new Vector3Int(max.x, max.y, min.z);
            case Directions.South:
                return new Vector3Int(min.x, max.y, max.z);
            case Directions.West:
                return max;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds3Int Translate(Vector3Int translation)
    {
        return new Bounds3Int(min + translation, max + translation);
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
