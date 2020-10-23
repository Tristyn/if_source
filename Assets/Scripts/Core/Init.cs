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

    /// <summary>
    /// Called before building a save file.
    /// </summary>
    public static event Action PreSave
    {
        add => AddListener(ref preSave, value);
        remove => preSave -= value;
    }
    static event Action preSave;

    /// <summary>
    /// Called after building a save file.
    /// </summary>
    public static event Action PostSave
    {
        add => AddListener(ref postSave, value);
        remove => postSave -= value;
    }
    static event Action postSave;

    /// <summary>
    /// Called before loading a save file.
    /// </summary>
    public static event Action PreLoad
    {
        add => AddListener(ref preLoad, value);
        remove => preLoad -= value;
    }
    static event Action preLoad;

    /// <summary>
    /// Called after loading a save file, and Save structs have been applied.
    /// </summary>
    public static event Action PostLoad
    {
        add => AddListener(ref postLoad, value);
        remove => postLoad -= value;
    }
    static event Action postLoad;

    /// <summary>
    /// Called after all loading is complete and systems are updated and can interact.
    /// </summary>
    public static event Action LoadComplete
    {
        add => AddListener(ref loadComplete, value);
        remove => loadComplete -= value;
    }
    static event Action loadComplete;



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

    static void AddListener(ref Action actions, Action listener)
    {
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

    public static void InvokePreSave()
    {
        preSave?.Invoke();
    }

    public static void InvokePostSave()
    {
        postSave?.Invoke();
    }

    public static void InvokePreLoad()
    {
        preLoad?.Invoke();
    }

    public static void InvokePostLoad()
    {
        postLoad?.Invoke();
    }

    public static void InvokeLoadComplete()
    {
        loadComplete?.Invoke();
    }
}
