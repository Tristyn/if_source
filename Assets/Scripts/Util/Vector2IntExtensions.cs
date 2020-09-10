using System.Runtime.CompilerServices;
using UnityEngine;

public static class Vector2IntExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int ToVector3XZ(this Vector2Int vector)
    {
        return new Vector3Int(vector.x, 0, vector.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int ToVector3XY(this Vector2Int vector)
    {
        return new Vector3Int(vector.x, vector.y, 0);
    }
}
