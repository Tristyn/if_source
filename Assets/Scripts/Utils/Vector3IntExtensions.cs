using UnityEngine;

public static class Vector3IntExtensions
{
    public static Vector3 ToTilePosition(this Vector3Int vector)
    {
        return new Vector3(vector.x + 0.5f, vector.y, vector.z + 0.5f);
    }

    public static Bounds3Int ToBounds(this Vector3Int vector)
    {
        return new Bounds3Int(vector, vector);
    }
}
