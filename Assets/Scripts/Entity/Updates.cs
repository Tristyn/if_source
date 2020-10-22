using System;
using System.Collections.Generic;

public static class Updates
{
    public static BackgroundMusic backgroundMusic;
    public static TouchInput touchInput;
    public static Picker picker;
    public static AutoSaveLoad autoSaveLoad;

    public static FastRemoveList<UILinkConveyorButton> linkConveyorButtons = NewUpdateList<UILinkConveyorButton>(16);
    public static FastRemoveList<MachineDropper> machineDroppers = NewUpdateList<MachineDropper>(1);

    public static OverviewCameraController overviewCameraController;
    public static CameraShake cameraShake;

    public static FastRemoveList<Conveyor> conveyors = NewUpdateList<Conveyor>(128);
    public static FastRemoveList<MachinePurchaser> machinePurchasers = NewUpdateList<MachinePurchaser>(16);
    public static FastRemoveList<MachineSeller> machineSellers = NewUpdateList<MachineSeller>(16);
    public static FastRemoveList<MachineAssembler> machineAssemblers = NewUpdateList<MachineAssembler>(16);
    public static FastRemoveList<MachinePlacer> machinePlacers = NewUpdateList<MachinePlacer>(32);
    public static CampaignGoals campaignGoals;

    static Dictionary<Type, object> componentToUpdateList = new Dictionary<Type, object>();
    static Dictionary<Type, object> componentToFixedUpdateList = new Dictionary<Type, object>();

    private static FastRemoveList<T> NewUpdateList<T>(int capacity)
    {
        FastRemoveList<T> list = new FastRemoveList<T>(capacity);
        if (typeof(T) is IUpdate)
        {
            componentToUpdateList.Add(typeof(T), list);
        }
        if (typeof(T) is IFixedUpdate)
        {
            componentToFixedUpdateList.Add(typeof(T), list);
        }
        return list;
    }

    public static FastRemoveList<T> GetUpdate<T>() where T : IUpdate
    {
        return componentToUpdateList[typeof(T)] as FastRemoveList<T>;
    }
    public static FastRemoveList<T> GetFixedUpdate<T>() where T : IFixedUpdate
    {
        return componentToFixedUpdateList[typeof(T)] as FastRemoveList<T>;
    }
}
