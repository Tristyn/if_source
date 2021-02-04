using System;
using System.Collections.Generic;

public static class Entities
{
    public static BackgroundMusic backgroundMusic;
    public static TouchInput touchInput;
    public static Picker picker;
    public static AutoSaveLoad autoSaveLoad;

    public static UpdateList<UILinkConveyorButton> linkConveyorButtons = NewUpdateList<UILinkConveyorButton>(16);
    public static UpdateList<UISelectMachineButton> uiSelectMachineButtons = NewUpdateList<UISelectMachineButton>(32);
    public static UpdateList<MachineDropper> machineDroppers = NewUpdateList<MachineDropper>(1);

    public static OverviewCameraController overviewCameraController;
    public static CameraShake cameraShake;

    public static UpdateList<Conveyor> conveyors = NewUpdateList<Conveyor>(128);
    public static UpdateList<MachinePurchaser> machinePurchasers = NewUpdateList<MachinePurchaser>(16);
    public static UpdateList<MachineSeller> machineSellers = NewUpdateList<MachineSeller>(16);
    public static UpdateList<MachineAssembler> machineAssemblers = NewUpdateList<MachineAssembler>(16);
    public static UpdateList<MachinePlacer> machinePlacers = NewUpdateList<MachinePlacer>(32);
    public static PuzzleGoals puzzleGoals;

    static Dictionary<Type, object> componentToUpdateList = new Dictionary<Type, object>();
    static Dictionary<Type, object> componentToFixedUpdateList = new Dictionary<Type, object>();

    private static UpdateList<T> NewUpdateList<T>(int capacity)
    {
        UpdateList<T> list = new UpdateList<T>(capacity);
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

    public static UpdateList<T> GetUpdate<T>() where T : IUpdate
    {
        return componentToUpdateList[typeof(T)] as UpdateList<T>;
    }
    public static UpdateList<T> GetFixedUpdate<T>() where T : IFixedUpdate
    {
        return componentToFixedUpdateList[typeof(T)] as UpdateList<T>;
    }
}
