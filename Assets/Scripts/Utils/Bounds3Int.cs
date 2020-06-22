using System.Collections.Generic;
using UnityEngine;

public struct Bounds3Int 
{
    public Vector3Int min;
    public Vector3Int max;

    public Bounds3Int(Vector3Int min, Vector3Int max)
    {
        this.min = min;
        this.max = max;
    }

    public Vector3 center => min + ((Vector3)(max - min)) / 2;

    public bool Contains(Vector3Int position)
    {
        return position.x >= min.x && position.y >= min.y && position.z >= min.z
            && position.x <= max.x && position.y <= max.y && position.z <= max.z;
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
        for (int x = min.x; x <= max.x; x++)
        {
            yield return (new Vector3Int(x, min.y, min.z - 1), new Vector3Int(x, min.y, min.z));
            yield return (new Vector3Int(x, min.y, max.z + 1), new Vector3Int(x, min.y, max.z));
        }
        for (int z = min.z; z <= max.z; z++)
        {
            yield return (new Vector3Int(min.x - 1, min.y, z), new Vector3Int(min.x, min.y, z));
            yield return (new Vector3Int(max.x + 1, min.y, z), new Vector3Int(max.x, min.y, z));
        }
    }
}
