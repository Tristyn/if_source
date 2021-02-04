using UnityEngine;

public sealed class GameVars : Singleton<GameVars>
{
    static Vars vars;

    protected override void Awake()
    {
        base.Awake();
        Init.Bind += () => vars = ScriptableObjects.instance.vars;
    }

    public static float GetMachineCost(MachineInfo machineInfo, long count)=> vars.MachineCostGrowth.At(machineInfo.cost, count);
    public static long GetSpacePlatformCost(long count) => vars.platformCostGrowth.At(count);
    public static long GetPlatformArea(long count) => vars.platformAreaGrowth.At(count);
    public static long GetSpacePlatformMinSize(long count) => vars.platformMinSizeGrowth.At(count);
    public static long GetSpacePlatformMaxSize(long count) => vars.platformMaxSizeGrowth.At(count);
}