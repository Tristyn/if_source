using UnityEngine;
using System;

public class Init : MonoBehaviour
{
    public static bool Initializing = true;

    static event Action configure;
    public static event Action Configure
    {
        add => AddListener(ref configure, value);
        remove => configure -= value;
    }
    static event Action bind;
    public static event Action Bind
    {
        add => AddListener(ref bind, value);
        remove => bind -= value;
    }

    static void AddListener(ref Action actions, Action listener)
    {
        if(Initializing)
        {
            actions = actions += listener;
        }
        else
        {
            listener();
        }
    }

    void Start()
    {
        configure?.Invoke();
        bind?.Invoke();

        configure = null;
        bind = null;
    }
}
