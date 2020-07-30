using UnityEngine;

public sealed class MainCamera : Singleton<Camera>
{
    public static Transform instanceTransform;

    protected override void Awake()
    {
        instance = GetComponent<Camera>();
        instanceTransform = instance.transform;
    }
}