using UnityEngine;

public static class Vector3IntExtensions
{
    public static Vector3 RoundToTileCenter(this Vector3Int vector3)
    {
        return new Vector3(Mathf.Floor(vector3.x) + 0.5f, Mathf.Floor(vector3.y), Mathf.Floor(vector3.z) + 0.5f);
    }

    public static Bounds3Int ToBounds(this Vector3Int vector)
    {
        return new Bounds3Int(vector, vector);
    }

    public static Vector3Int Add(this Vector3Int vector, int value)
    {
        return new Vector3Int(vector.x + value, vector.y + value, vector.z + value);
    }
}