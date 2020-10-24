using UnityEngine;
using System;

public sealed class Init : MonoBehaviour
{
    enum InitStep
    {
        None = 0,
        Configure = 1,
        Bind = 2,
        StartupLoad = 3,
        Complete = 4
    }

    public static bool initialized = false;
    static InitStep initStep;

    /// <summary>
    /// Last minute configuration before bind is called
    /// </summary>
    public static event Action Configure
    {
        add => AddListener(ref configure, value, InitStep.Configure);
        remove => configure -= value;
    }
    static event Action configure;

    /// <summary>
    /// Bind and register to objects in the scene
    /// </summary>
    public static event Action Bind
    {
        add => AddListener(ref bind, value, InitStep.Bind);
        remove => bind -= value;
    }
    static event Action bind;

    /// <summary>
    /// Load the save file at startup
    /// </summary>
    public static event Action StartupLoad
    {
        add => AddListener(ref startupLoad, value, InitStep.StartupLoad);
        remove => startupLoad -= value;
    }
    static event Action startupLoad;

    static void AddListener(ref Action actions, Action listener, InitStep initStep)
    {
        if (Init.initStep >= initStep)
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

    private void Awake()
    {
        AotMethods.Ensure();
        JsonConfiguration.Configure();
    }

    void Start()
    {
        initStep = InitStep.Configure;
        configure?.Invoke();

        initStep = InitStep.Bind;
        bind?.Invoke();

        initStep = InitStep.StartupLoad;
        startupLoad?.Invoke();

        initStep = InitStep.Complete;
        initialized = true;
        configure = null;
        bind = null;
        startupLoad = null;
    }
}
