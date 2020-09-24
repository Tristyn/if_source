using UnityEngine;

public static class GameModeInitializer
{
    public static void InitializeSandbox()
    {
        InitializeCommon();
        CurrencySystem.instance.SetMoney(100);
        CurrencySystem.instance.SetItemsCostMoney(true);
        Bounds3Int bounds = new Bounds3Int(new Vector3Int(-10, 0, -10), new Vector3Int(9, 0, 9));
        Floor floor = ObjectPooler.instance.Get<Floor>();
        floor.Initialize(bounds);
        LandSystem.instance.AddLandParcel(new LandParcel
        {
            flags = LandParcelFlags.Valid,
            bounds = new[] { bounds }
        });
    }

    public static void InitializeCampaign()
    {
        InitializeCommon();
        CurrencySystem.instance.SetItemsCostMoney(false);
        CampaignGoals.instance.BeginCampaign();
    }

    static void InitializeCommon()
    {
        GameTime.StartOfTime();
        OverviewCameraController.instance.SetZoomIncrement(int.MaxValue);
        OverviewCameraController.instance.Rotate(90f);
    }
}
