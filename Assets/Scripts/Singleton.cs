using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T instance;

    protected virtual void Awake()
    {
        instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        instance = null;
    }
}
