using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public sealed class Profile
{
    public string version;
    public PlayFabLogin.Profile Login;
}

public static class ProfileLoader
{
    public sealed class ProfileOptions
    {
        public ProfileOptions()
        {
            directory = Path.Combine(Application.persistentDataPath, "saves");
            fileName = "profile";
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

    static void BackupCorruptProfile(string path)
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

    public static void NewProfile()
    {
        Profile newProfile = new Profile();
        Load(newProfile);
    }

    public static void Save(ProfileOptions profileOptions = null)
    {
        try
        {
            if (profileOptions == null)
            {
                profileOptions = new ProfileOptions();
            }
            string path = profileOptions.path;
            Directory.CreateDirectory(profileOptions.directory);
            string saveJson = BuildProfileJson(profileOptions);
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

    public static bool Load(ProfileOptions profileOptions = null)
    {
        if (profileOptions == null)
        {
            profileOptions = new ProfileOptions();
        }

        string path = profileOptions.path;
        if (File.Exists(path))
        {
            string profileJson = File.ReadAllText(path);
            if (!LoadFromJson(profileJson))
            {
                BackupCorruptProfile(path);
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
            Profile profile = JsonConvert.DeserializeObject<Profile>(json);
            Load(profile);
            return true;
        }
        catch (Exception ex)
        {
            ApplicationException log = new ApplicationException("Exception when loading corrupt profile file. Now attemping to load the starter profile.", ex);
            Debug.LogException(log);
            return false;
        }
    }

    public static string BuildProfileJson(ProfileOptions profileOptions)
    {
        Profile profile = BuildProfile();
        return JsonConvert.SerializeObject(profile, profileOptions.formatting);
    }

    static Profile BuildProfile()
    {
        Profile profile = new Profile
        {
            version = Migrations.version,
            Login = PlayFabLogin.instance.profile
        };
        return profile;
    }

    static void Load(Profile profile)
    {
        PlayFabLogin.instance.profile = profile.Login;
    }
}