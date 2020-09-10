using UnityEngine;

/// <summary>
/// Enumerates through 2D coordinates that it passes through using its origin and direction.
/// </summary>
public struct PixelTraversal
{
    public Vector2Int position;
    Vector2Int step;

    float tMaxX;
    float tMaxY;

    float tDeltaX;
    float tDeltaY;

    public PixelTraversal(Vector2 origin, Vector2 direction)
    {
        // Cube containing origin point.
        position = Mathx.FloorToInt(origin);

        // Avoids an infinite loop.
        if (direction.x == 0 && direction.y == 0)
            direction.x = 1;

        // Direction to increment x,y,z when stepping.
        step = new Vector2Int(
            Sign(direction.x),
            Sign(direction.y));

        // See description above. The initial values depend on the fractional
        // part of the origin.
        tMaxX = Intbound(origin.x, direction.x);
        tMaxY = Intbound(origin.y, direction.y);

        // The change in t when taking a step (always positive).
        tDeltaX = step.x / direction.x;
        tDeltaY = step.y / direction.y;
    }

    // TODO: add options for setting dimension limits

    public void Step()
    {
        // an implementation of http://www.cse.yorku.ca/~amana/research/grid.pdf

        // tMaxX stores the t-value at which we cross a cube boundary along the
        // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
        // chooses the closest cube boundary. Only the first case of the four
        // has been commented in detail.

        if (tMaxX < tMaxY)
        {
            // Update which cube we are now in.
            position.x += step.x;
            // Adjust tMaxX to the next X-oriented boundary crossing.
            tMaxX += tDeltaX;
        }
        else
        {
            // Identical to the second case, repeated for simplicity in
            // the conditionals.
            position.y += step.y;
            tMaxY += tDeltaY;
        }
    }

    static float Intbound(float s, float ds)
    {
        // Find the smallest positive t such that s+t*ds is an integer.
        if (ds < 0)
            return Intbound(-s, -ds);

        s = WeirdMod(s, 1);
        // problem is now s+t*ds = 1
        return (1 - s) / ds;
    }

    static float WeirdMod(float value, float modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    static int Sign(float val)
    {
        return val > 0 ? 1 : val < 0 ? -1 : 0;
    }
}