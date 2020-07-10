using UnityEngine;

public class MainCamera : Singleton<Camera>
{
    protected override void Awake()
    {
        instance = GetComponent<Camera>();
    }
}