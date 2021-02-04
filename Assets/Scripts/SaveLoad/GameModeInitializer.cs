using UnityEngine;

public static class GameModeInitializer
{
    public static void InitializeFreePlay()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(true);
        ProgressionSystem.instance.ApplyStartingProgressions();

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-6, 0, -8), new Vector3Int(5, 0, 7)) };
        SpacePlatform.Create(bounds, Color.white);
    }

    public static void InitializeSandbox()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);
        MachineUnlockSystem.instance.UnlockAll();

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-10, 0, -10), new Vector3Int(9, 0, 9)) };
        SpacePlatform.Create(bounds, Color.white);
    }

    public static void InitializePuzzles()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);
        ProgressionSystem.instance.ApplyStartingProgressions();
        PuzzleGoals.instance.BeginCampaign();
    }

    static void InitializeCommon()
    {
        SaveLoad.NewSaveGame();
        GameTime.StartOfTime();
        OverviewCameraController.instance.SetEnabled(true);
    }

    static void InitializeRooms()
    {
        for (int i = 1; i <= 50; ++i)
        {
            Color floorColor = new Color(Random.Range(128f, 255f), Random.Range(128f, 255f), Random.Range(128f, 255f));
            AddonParameters parameters = new AddonParameters
            {
                minArea = GameVars.GetPlatformArea(i),
                primarySizeMin = GameVars.GetSpacePlatformMinSize(i) + 2,
                primarySizeMax = GameVars.GetSpacePlatformMaxSize(i) + 2,
                secondarySizeMin = GameVars.GetSpacePlatformMinSize(i),
                secondarySizeMax = GameVars.GetSpacePlatformMaxSize(i)
            };
            Bounds3Int[] bounds = AddonGen.GenerateAddon(parameters);
            SpacePlatform.Create(bounds, floorColor);
        }
    }
}
