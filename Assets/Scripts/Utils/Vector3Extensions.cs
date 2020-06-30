using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3Int RoundToTile(this Vector3 vector3)
    {
        return new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
    }

    public static Vector3 RoundToTileCenter(this Vector3 vector3)
    {
        return new Vector3(Mathf.Floor(vector3.x) + 0.5f, Mathf.Floor(vector3.y), Mathf.Floor(vector3.z) + 0.5f);
    }

    public static Vector3 RotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public static Bounds3Int PositionToBounds(this Vector3 tileCenter, Vector3Int size)
    {
        Vector3Int min = (tileCenter - (Vector3)size/2).RoundToTile();
        Bounds3Int bounds = new Bounds3Int(min, min + size);
        return bounds;
    }
}
