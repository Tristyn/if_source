using System;

public static class Migrations
{
    struct Migration
    {
        public string version;
        public Action<SaveFile> action;
    }

    static Migration[] migrations = new Migration[]
    {
        new Migration { action = Migration_0_1_8_4.Migrate, version = "0_1_8_4"}
    };

    public static string version = migrations[migrations.Length - 1].version;

    public static void Migrate(SaveFile saveFile)
    {
        int i = GetMigrationIndex(saveFile.version);
        for(int len = migrations.Length; i < len; ++i)
        {
            migrations[i].action(saveFile);
            saveFile.version = migrations[i].version;
        }
    }

    static int GetMigrationIndex(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return 0;
        }
        for (int i = 0, len = migrations.Length; i < len; ++i)
        {
            if (migrations[i].version == version)
            {
                return i + 1;
            }
        }
        return 0;
    }
}
