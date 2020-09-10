using UnityEngine;

public static class Mathx
{
    /// <summary>
    /// Converts a count of populated bits to it's corresponding bitmask. e.g.
    /// 0 => 0x00000000
    /// 1 => 0x00000001
    /// 2 => 0x00000011
    /// 3 => 0x00000111
    /// 4 => 0x00001111
    /// 5 => 0x00011111
    /// </summary>
    /// <param name="numBits"></param>
    /// <returns></returns>
    public static uint BitCountToMask(int numBits)
    {
        return ~(~0u << numBits);
    }

    public static Vector3Int RandomRange(Vector3Int min, Vector3Int max)
    {
        return new Vector3Int(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z));
    }

    public static Vector3Int FloorToInt(Vector3 vector)
    {
        return new Vector3Int(
            Mathf.FloorToInt(vector.x),
            Mathf.FloorToInt(vector.y),
            Mathf.FloorToInt(vector.z));
    }

    public static Vector2Int FloorToInt(Vector2 vector)
    {
        return new Vector2Int(
            Mathf.FloorToInt(vector.x),
            Mathf.FloorToInt(vector.y));
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}
