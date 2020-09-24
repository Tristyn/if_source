using UnityEngine;

public sealed class AutoSaveLoad : Singleton<AutoSaveLoad>
{
    public float autoSaveInterval = 30f;
    float nextAutoSaveTime;
    bool autosaveDisable;

    protected override void Awake()
    {
        base.Awake();
        nextAutoSaveTime = GameTime.time + autoSaveInterval;
        Init.StartupLoad += StartupLoad;
        Init.LoadComplete += LoadComplete;
        Application.focusChanged += FocusChanged;
        Application.quitting += Save;

        var saveDirectory = new SaveLoad.SaveOptions().directory;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Init.LoadComplete -= LoadComplete;
        Application.focusChanged -= FocusChanged;
        Application.quitting -= Save;
    }

    void LoadComplete()
    {
        nextAutoSaveTime = GameTime.time + autoSaveInterval;
    }

    public void DoUpdate()
    {
        if (GameTime.time > nextAutoSaveTime && !autosaveDisable)
        {
            nextAutoSaveTime = GameTime.time + autoSaveInterval;
            SaveLoad.Save();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveLoad.Save();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            SaveLoad.Load();
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            autosaveDisable = true;
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            autosaveDisable = false;
        }
#endif
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveLoad.Save();
        }
    }

    void FocusChanged(bool focused)
    {
        if (!focused)
        {
            Save();
        }
    }

    void Save()
    {
        SaveLoad.Save();
    }

    void StartupLoad()
    {
        if (!SaveLoad.Load())
        {
            SaveLoad.NewSaveGame();
        }
    }
}
