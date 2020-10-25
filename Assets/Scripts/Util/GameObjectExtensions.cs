using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetAllLayers(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        Transform transform = gameObject.transform;
        for (int i = 0, len = transform.childCount; i < len; ++i)
        {
            transform.GetChild(i).gameObject.SetAllLayers(layer);
        }
    }
}
