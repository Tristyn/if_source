using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class SaveLoad
{
    [Serializable]
    public sealed class SaveFile
    {
        public GameTime.Save gameTime;
        public CurrencySystem.Save currency;
        public ConveyorSystem.Save conveyor;
        public MachineSystem.Save machine;
        public TileSelectionManager.Save tileSelection;
        public InterfaceSelectionManager.Save interfaceSelection;
        public BackgroundMusic.Save backgroundMusic;
        public OverviewCameraController.Save overviewCameraController;
    }

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
                Init.InvokePreLoad();
                StarterSave.SuperimposeStarterSave();
                Init.InvokeLoadComplete();
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
            gameTime = GameTime.save,
            currency = CurrencySystem.instance.save,
            conveyor = ConveyorSystem.instance.save,
            machine = MachineSystem.instance.save,
            tileSelection = TileSelectionManager.instance.save,
            overviewCameraController = OverviewCameraController.instance.save
        };
        BackgroundMusic.instance.GetSave(out save.backgroundMusic);
        InterfaceSelectionManager.instance.GetSave(out save.interfaceSelection);
        Init.InvokePostSave();

        return save;
    }

    static void Load(SaveFile saveFile)
    {
        Init.InvokePreLoad();
        Physics.SyncTransforms();
        GameTime.save = saveFile.gameTime;
        CurrencySystem.instance.save = saveFile.currency;
        ConveyorSystem.instance.save = saveFile.conveyor;
        MachineSystem.instance.save = saveFile.machine;
        TileSelectionManager.instance.save = saveFile.tileSelection;
        OverviewCameraController.instance.save = saveFile.overviewCameraController;
        InterfaceSelectionManager.instance.SetSave(in saveFile.interfaceSelection);
        BackgroundMusic.instance.SetSave(in saveFile.backgroundMusic);
        Init.InvokePostLoad();
        Init.InvokeLoadComplete();
    }
}
