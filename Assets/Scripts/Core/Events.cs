using System;

public static class Events
{
    public static Action<MachineInfo> MachineUnlocked;
    public static Action<Machine> MachineCreated;
    public static Action<Machine> MachineLanded;
    public static Action<Machine> MachineDeleted;
    public static Action<MenuState> MenuStateChanged;
}
