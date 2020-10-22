using System.Runtime.CompilerServices;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3Int RoundDown(this Vector3 vector3)
    {
        return new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
    }

    public static Vector3 RoundToTileCenter(this Vector3 vector3)
    {
        return new Vector3(Mathf.Floor(vector3.x) + 0.5f, Mathf.Floor(vector3.y), Mathf.Floor(vector3.z) + 0.5f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int ToVector2XZ(this Vector3Int vector)
    {
        return new Vector2Int(vector.x, vector.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int ToVector2XY(this Vector3Int vector)
    {
        return new Vector2Int(vector.x, vector.y);
    }

    public static Bounds3Int PositionBottomToBounds(this Vector3 tileCenter, Vector3Int size)
    {
        Vector3 min = (tileCenter - (Vector3)size / 2);

        Vector3Int boundsMin = new Vector3Int(
            Mathf.RoundToInt(min.x),
            Mathf.FloorToInt(tileCenter.y),
            Mathf.RoundToInt(min.z));
        Bounds3Int bounds = Bounds3Int.Create(boundsMin, size);
        return bounds;
    }

    public static Vector3 RotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Subtract(this Vector3 vector, float value)
    {
        return new Vector3(vector.x - value, vector.y - value, vector.z - value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Scale(this Vector3Int vector, float value)
    {
        return new Vector3(vector.x * value, vector.y * value, vector.z * value);
    }

    public static bool AllLessThan(this Vector3 vector, float value)
    {
        return vector.x < value
            && vector.y < value
            && vector.z < value;
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }
}
