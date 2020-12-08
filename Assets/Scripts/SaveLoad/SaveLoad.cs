using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class SaveFile
{
    public string version;
    public GameTime.Save gameTime;
    public CurrencySystem.Save currency;
    public ConveyorSystem.Save conveyor;
    public MachineSystem.Save machine;
    public TileSelectionManager.Save tileSelection;
    public OverviewCameraController.Save overviewCameraController;
    public Analytics.Save analytics;
    public BackgroundMusic.Save backgroundMusic;
    public InterfaceSelectionManager.Save interfaceSelection;
    public MachineGroupAchievements.Save machineGroupAchievements;
    public MachineUnlockSystem.Save machineUnlocks;
    public ProgressionStore.Save progressionSystem;
    public SpacePlatform.Saves spacePlatforms;
}

public static class SaveLoad
{
    public sealed class SaveOptions
    {
        public SaveOptions()
        {
            directory = Path.Combine(Application.persistentDataPath, "saves");
            fileName = "save";
            fileExtension = ".json";
#if UNITY_EDITOR
            formatting = Formatting.Indented;
#endif
        }

        public Formatting formatting;
        public string directory;
        public string fileName;
        public string fileExtension;

        public string path => Path.Combine(directory, fileName + fileExtension);
    }

    /// <summary>
    /// Called before building a save file.
    /// </summary>
    public static event Action PreSave;

    /// <summary>
    /// Called after building a save file.
    /// </summary>
    public static event Action PostSave;

    /// <summary>
    /// Called before loading a save file.
    /// </summary>
    public static event Action PreLoad;

    /// <summary>
    /// Called after loading a save file, and Save structs have been applied.
    /// </summary>
    public static event Action PostLoad;

    /// <summary>
    /// Called after all loading is complete and systems are updated and can interact.
    /// </summary>
    public static event Action LoadComplete;

    static void BackupCorruptSave(string path)
    {
        string dir = Path.GetDirectoryName(path);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string fileExt = Path.GetExtension(path);

        string newPath;
        int i = 0;
        do
        {
            ++i;
            newPath = Path.Combine(dir, fileName + "_corrupt_" + i.ToString("000") + fileExt);
        }
        while (File.Exists(newPath));
        File.Move(path, newPath);
    }

    public static void NewSaveGame()
    {
        SaveFile newSave = new SaveFile();
        Load(newSave);
    }

    public static void Save(SaveOptions saveOptions = null)
    {
        try
        {
            if (saveOptions == null)
            {
                saveOptions = new SaveOptions();
            }
            string path = saveOptions.path;
            Directory.CreateDirectory(saveOptions.directory);
            string saveJson = BuildSaveJson(saveOptions);
            string tempFilePath = path + ".tmp";
            File.WriteAllText(tempFilePath, saveJson);
            if (File.Exists(path))
            {
                File.Replace(tempFilePath, path, null);
            }
            else
            {
                File.Move(tempFilePath, path);
            }
            ProfileLoader.Save();
#if UNITY_WEBGL && !UNITY_EDITOR
#pragma warning disable CS0618 // Type or member is obsolete
            // Required to flush the WebGL file cache to IndexedDB. This will annoyingly log the command
            Application.ExternalEval("_JS_FileSystem_Sync();");
#pragma warning restore CS0618 // Type or member is obsolete
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static bool Load(SaveOptions saveOptions = null)
    {
        if (saveOptions == null)
        {
            saveOptions = new SaveOptions();
        }

        ProfileLoader.Load();
        string path = saveOptions.path;
        if (File.Exists(path))
        {
            string saveJson = File.ReadAllText(path);
            if (!LoadFromJson(saveJson))
            {
                BackupCorruptSave(path);
                return false;
            }
            return true;
        }
        return false;
    }

    public static bool LoadFromJson(string json)
    {
        try
        {
            SaveFile saveFile = JsonConvert.DeserializeObject<SaveFile>(json);
            Load(saveFile);
            return true;
        }
        catch (Exception ex)
        {
            ApplicationException log = new ApplicationException("Exception when loading corrupt save file. Now attemping to load the starter save.", ex);
            Debug.LogException(log);
            return false;
        }
    }

    public static string BuildSaveJson(SaveOptions saveOptions)
    {
        SaveFile saveFile = BuildSave();
        return JsonConvert.SerializeObject(saveFile, saveOptions.formatting);
    }

    static SaveFile BuildSave()
    {
        PreSave?.Invoke();
        SaveFile save = new SaveFile
        {
            version = Migrations.version,
            gameTime = GameTime.save,
            currency = CurrencySystem.instance.save,
            conveyor = ConveyorSystem.instance.save,
            machine = MachineSystem.instance.save,
            tileSelection = TileSelectionManager.instance.save,
            overviewCameraController = OverviewCameraController.instance.save,
            analytics = Analytics.instance.save,
        };
        BackgroundMusic.instance.GetSave(out save.backgroundMusic);
        InterfaceSelectionManager.instance.GetSave(out save.interfaceSelection);
        MachineGroupAchievements.instance.GetSave(out save.machineGroupAchievements);
        MachineUnlockSystem.instance.GetSave(out save.machineUnlocks);
        ProgressionStore.instance.GetSave(out save.progressionSystem);
        SpacePlatform.GetSave(out save.spacePlatforms);
        PostSave?.Invoke();

        return save;
    }

    static void Load(SaveFile saveFile)
    {
        Migrations.Migrate(saveFile);
        PreLoad?.Invoke();
        GameTime.save = saveFile.gameTime;
        CurrencySystem.instance.save = saveFile.currency;
        ConveyorSystem.instance.save = saveFile.conveyor;
        MachineSystem.instance.save = saveFile.machine;
        TileSelectionManager.instance.save = saveFile.tileSelection;
        OverviewCameraController.instance.save = saveFile.overviewCameraController;
        Analytics.instance.save = saveFile.analytics;
        BackgroundMusic.instance.SetSave(in saveFile.backgroundMusic);
        InterfaceSelectionManager.instance.SetSave(in saveFile.interfaceSelection);
        MachineGroupAchievements.instance.SetSave(in saveFile.machineGroupAchievements);
        MachineUnlockSystem.instance.SetSave(in saveFile.machineUnlocks);
        ProgressionStore.instance.SetSave(in saveFile.progressionSystem);
        SpacePlatform.SetSave(in saveFile.spacePlatforms);
        PostLoad?.Invoke();
        LoadComplete?.Invoke();
    }
}
