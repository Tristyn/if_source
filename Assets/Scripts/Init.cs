using UnityEngine;
using System;
using UnityEngine.Assertions;

public class Init : MonoBehaviour
{
    public static bool initialized = false;
    static bool initializing = false;

    /// <summary>
    /// Last minute configuration before bind is called
    /// </summary>
    public static event Action Configure
    {
        add => AddListener(ref configure, value);
        remove => configure -= value;
    }
    static event Action configure;

    /// <summary>
    /// Bind and register to objects in the scene
    /// </summary>
    public static event Action Bind
    {
        add => AddListener(ref bind, value);
        remove => bind -= value;
    }
    static event Action bind;

    static void AddListener(ref Action actions, Action listener)
    {
        Assert.IsFalse(initializing, "object is registering to Init while Init callbacks are firing.");
        if (initializing)
        {
            listener();
        }

        if (initialized)
        {
            listener();
        }
        else
        {
            actions = actions += listener;
        }
    }

    void Start()
    {
        initializing = true;
        configure?.Invoke();
        bind?.Invoke();
        initializing = false;
        initialized = true;

        configure = null;
        bind = null;
    }
}
