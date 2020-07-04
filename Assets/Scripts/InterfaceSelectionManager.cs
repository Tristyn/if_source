public enum InterfaceMode
{
    Conveyor, Machine
}

public struct InterfaceState
{
    public InterfaceMode mode;
    public MachineInfo machineInfo;

}

public class InterfaceSelectionManager : Singleton<InterfaceSelectionManager>
{
    public InterfaceState state;
    public UISelectMachinesButton selectMachinesButton;

    public void SetSelection(MachineInfo machineInfo)
    {
        state = new InterfaceState
        {
            mode = InterfaceMode.Machine,
            machineInfo = machineInfo
        };
        UpdateSelection();
    }

    public void SetSelectionConveyor()
    {
        state = new InterfaceState
        {
            mode = InterfaceMode.Conveyor
        };
        UpdateSelection();
    }

    void UpdateSelection()
    {
        selectMachinesButton.CollapseList();
        selectMachinesButton.OnSelectionChanged();
        selectMachinesButton.PlaySelectMachineAudio();
    }
}
