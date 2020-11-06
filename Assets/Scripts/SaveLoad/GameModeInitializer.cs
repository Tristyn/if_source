using UnityEngine;

public static class GameModeInitializer
{
    public static void InitializeFreePlay()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(true);
        ProgressionSystem.instance.ApplyStartingProgressions();

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-6, 0, -8), new Vector3Int(5, 0, 7)) };
        SpacePlatform spacePlatform = new SpacePlatform();
        spacePlatform.save.bounds = bounds;
        spacePlatform.Initialize();
    }

    public static void InitializeSandbox()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);
        MachineUnlockSystem.instance.UnlockAll();

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-10, 0, -10), new Vector3Int(9, 0, 9)) };
        SpacePlatform spacePlatform = new SpacePlatform();
        spacePlatform.save.bounds = bounds;
        spacePlatform.Initialize();
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
        MenuController.instance.SetState(MenuState.Closed);
        GameTime.StartOfTime();
        OverviewCameraController.instance.SetEnabled(true);
    }
}
