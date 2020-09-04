using UnityEngine;

public static class GameModeInitializer
{
    public static void InitializeSandbox()
    {
        InitializeCommon();
        CurrencySystem.instance.SetMoney(100);
        LandSystem.instance.AddLandParcel(new LandParcel
        {
            Flags = LandParcelFlags.Valid,
            bounds = new Bounds3Int[]
            {
                new Bounds3Int(new Vector3Int(-10,0,-10), new Vector3Int(10,0,10))
            }
        });
    }

    public static void InitializeCampaign()
    {
        InitializeCommon();
        LandSystem.instance.AddLandParcel(new LandParcel
        {
            Flags = LandParcelFlags.Valid,
            bounds = new Bounds3Int[]
            {
                new Bounds3Int(new Vector3Int(-10,0,-10), new Vector3Int(10,0,10))
            }
        });
    }

    static void InitializeCommon()
    {
        GameTime.StartOfTime();
        OverviewCameraController.instance.SetZoomIncrement(int.MaxValue);
    }
}
