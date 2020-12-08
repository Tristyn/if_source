using System;

public static class Events
{
    public static Action<MachineInfo> MachineUnlocked;
    public static Action<Machine> MachineCreated;
    public static Action<Machine> MachineLanded;
    public static Action<Machine> MachineDeleted;

    public static Action<SelectionState> SelectionChanged;
    public static Action<SelectionState> InterfaceSelectionChanged;
    public static Action<SelectionState> TileSelectionChanged;
    public static Action<SelectionState> SelectMachineButtonHovered;

    public static Action<MenuState> MenuStateChanged;
    public static Action<LoginState> LoginChanged;
}
