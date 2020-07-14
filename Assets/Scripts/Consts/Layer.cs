using UnityEngine;

public static class Layer
{
    static Layer()
    {
        for (int i = 0; i < maxLayers; ++i)
        {
            string layerName = LayerMask.LayerToName(i);
            layerMasks[i] = LayerMask.GetMask(layerName);
        }
    }

    public const int @default = 0;
    public const int transparentFX = 1;
    public const int ignoreRaycast = 2;
    public const int water = 4;
    public const int ui = 5;
    public const int floor = 8;
    public const int conveyor = 9;
    public const int machines = 10;

    static LayerMask[] layerMasks = new LayerMask[32];

    private const int maxLayers = 32;

    public static LayerMask GetMask(int layer)
    {
        return layerMasks[layer];
    }
}
