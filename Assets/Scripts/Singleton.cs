using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    public static T instance;

    protected virtual void Awake()
    {
        instance = (T)(object)this;
    }

    protected virtual void OnDestroy()
    {
        instance = (T)(object)null;
    }
}
