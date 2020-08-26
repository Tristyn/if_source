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
}
