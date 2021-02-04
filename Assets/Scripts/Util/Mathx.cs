using UnityEngine;

public static class Mathx
{
    // Exponential functions for game rules
    // 


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

    /// <summary>
    /// Population count a.k.a. hamming weight. Count the number of high bits in an integer
    /// </summary>
    public static int PopCount(ulong x)
    {
        x = (x & 0x5555555555555555) + ((x >> 1) & 0x5555555555555555); //put count of each  2 bits into those  2 bits 
        x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333); //put count of each  4 bits into those  4 bits 
        x = (x & 0x0f0f0f0f0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f0f0f0f0f); //put count of each  8 bits into those  8 bits 
        x = (x & 0x00ff00ff00ff00ff) + ((x >> 8) & 0x00ff00ff00ff00ff); //put count of each 16 bits into those 16 bits 
        x = (x & 0x0000ffff0000ffff) + ((x >> 16) & 0x0000ffff0000ffff); //put count of each 32 bits into those 32 bits 
        x = (x & 0x00000000ffffffff) + ((x >> 32) & 0x00000000ffffffff); //put count of each 64 bits into those 64 bits 
        return (int)x;
    }

    /// <summary>
    /// Population count a.k.a. hamming weight. Count the number of high bits in an integer
    /// </summary>
    public static int PopCount(uint x)
    {
        x = (x & 0x55555555) + ((x >> 1) & 0x55555555); //put count of each  2 bits into those  2 bits 
        x = (x & 0x33333333) + ((x >> 2) & 0x33333333); //put count of each  4 bits into those  4 bits 
        x = (x & 0x0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f); //put count of each  8 bits into those  8 bits 
        x = (x & 0x00ff00ff) + ((x >> 8) & 0x00ff00ff); //put count of each 16 bits into those 16 bits 
        x = (x & 0x0000ffff) + ((x >> 16) & 0x0000ffff); //put count of each 32 bits into those 32 bits 
        return (int)x;
    }

    public static void PopulateRange(int[] array, int start)
    {
        for (int i = 0, len = array.Length; i < len; ++i)
        {
            array[i] = start;
            ++start;
        }
    }

    /// <summary>
    /// Calculate the frustum length at a given distance from the camera.
    /// Supports calculating for horizontal or vertical fov given horiz or vertical frustum.
    /// </summary>
    public static float FrustumLengthAtDistance(float distance, float fieldOfView)
    {
        return 2.0f * distance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    /// <summary>
    /// Calculate the FOV needed to get a given frustum height at a given distance.
    /// Supports calculating for horizontal or vertical fov given horiz or vertical frustum.
    /// </summary>
    public static float FovAtDistanceAndFrustumLength(float distance, float frustumLength)
    {
        return 2.0f * Mathf.Atan(frustumLength * 0.5f / distance) * Mathf.Rad2Deg;
    }

    public static long RoundToInt(float val, long rounding)
    {
        return Mathf.RoundToInt(val / rounding) * rounding;
    }
}
