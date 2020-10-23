using UnityEngine;

public static class GameModeInitializer
{
    public static void InitializeFreePlay()
    {
        InitializeCommon();
        CurrencySystem.instance.SetMoney(500);
        CurrencySystem.instance.SetItemsCostMoney(true);

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-10, 0, -10), new Vector3Int(9, 0, 9)) };
        SpacePlatform spacePlatform = new SpacePlatform();
        spacePlatform.save.bounds = bounds;
        spacePlatform.Initialize();
    }
    public static void InitializeSandbox()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);

        Bounds3Int[] bounds = new[] { new Bounds3Int(new Vector3Int(-10, 0, -10), new Vector3Int(9, 0, 9)) };
        SpacePlatform spacePlatform = new SpacePlatform();
        spacePlatform.save.bounds = bounds;
        spacePlatform.Initialize();
    }

    public static void InitializePuzzles()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);
        PuzzleGoals.instance.BeginCampaign();
    }

    static void InitializeCommon()
    {
        GameTime.StartOfTime();
        OverviewCameraController.instance.SetZoomIncrement(int.MaxValue);
        OverviewCameraController.instance.Rotate(90f);
    }
}
