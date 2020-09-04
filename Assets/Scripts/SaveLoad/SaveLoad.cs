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
    public LandSystem.Save land;
    public ConveyorSystem.Save conveyor;
    public MachineSystem.Save machine;
    public TileSelectionManager.Save tileSelection;
    public OverviewCameraController.Save overviewCameraController;
    public Analytics.Save analytics;
    public MachineGroupAchievements.Save machineGroupProgression;
    public BackgroundMusic.Save backgroundMusic;
    public InterfaceSelectionManager.Save interfaceSelection;
    public MachineGroupAchievements.Save machineGroupAchievements;
    public ProgressionSystem.Save progressionSystem;
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
        UIMessageBox.MessageBox("Save and quit", "Are you sure you want to quit the game?", new UIMessageBoxAction[]{
            new UIMessageBoxAction
            {
                text = "Campaign",
                action = GameModeInitializer.InitializeCampaign
            },
            new UIMessageBoxAction
            {
                text="Sandbox",
                action = GameModeInitializer.InitializeSandbox
            }
        });
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
            SaveFile saveFile = BuildSave();
            string saveJson = JsonConvert.SerializeObject(saveFile, saveOptions.formatting);
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

        string path = saveOptions.path;
        if (File.Exists(path))
        {
            string saveJson = File.ReadAllText(path);
            SaveFile saveFile = JsonConvert.DeserializeObject<SaveFile>(saveJson);
            try
            {
                Load(saveFile);
            }
            catch (Exception ex)
            {
                ApplicationException log = new ApplicationException("Exception when loading corrupt save file. Now attemping to load the starter save.", ex);
                Debug.LogException(log);
                BackupCorruptSave(path);
                NewSaveGame();
            }
            return true;
        }
        return false;
    }

    static SaveFile BuildSave()
    {
        Init.InvokePreSave();
        SaveFile save = new SaveFile
        {
            version = Migrations.version,
            gameTime = GameTime.save,
            currency = CurrencySystem.instance.save,
            conveyor = ConveyorSystem.instance.save,
            machine = MachineSystem.instance.save,
            tileSelection = TileSelectionManager.instance.save,
            overviewCameraController = OverviewCameraController.instance.save,
            analytics = Analytics.instance.save
        };
        BackgroundMusic.instance.GetSave(out save.backgroundMusic);
        InterfaceSelectionManager.instance.GetSave(out save.interfaceSelection);
        MachineGroupAchievements.instance.GetSave(out save.machineGroupAchievements);
        ProgressionSystem.instance.GetSave(out save.progressionSystem);
        LandSystem.instance.GetSave(out save.land);
        Init.InvokePostSave();

        return save;
    }

    static void Load(SaveFile saveFile)
    {
        Migrations.Migrate(saveFile);
        Init.InvokePreLoad();
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
        ProgressionSystem.instance.SetSave(in saveFile.progressionSystem);
        LandSystem.instance.SetSave(in saveFile.land);
        Init.InvokePostLoad();
        Init.InvokeLoadComplete();
    }
}
