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

    public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null) // Explicit null check
        {
            return component;
        }
        return gameObject.AddComponent<T>();
    }
}
