using UnityEngine;

public enum InterfaceMode
{
    Conveyor, Machine
}

public struct InterfaceState
{
    public InterfaceMode mode;
    public MachineInfo machineInfo;

}

public sealed class InterfaceSelectionManager : Singleton<InterfaceSelectionManager>
{
    public InterfaceState state;
    public UISelectMachinesButton selectMachinesButton;

    public struct Save
    {
        public InterfaceMode mode;
        public string machineName;
    }

    public void GetSave(out Save save)
    {
        save.mode = state.mode;
        save.machineName = state.machineInfo ? state.machineInfo.machineName : null;
    }

    public void SetSave(in Save save)
    {
        if (save.mode == InterfaceMode.Conveyor)
        {
            SetSelectionConveyor();
        }
        else if (!string.IsNullOrEmpty(save.machineName))
        {
            MachineInfo machineInfo = ScriptableObjects.instance.GetMachineInfo(save.machineName);
            if (machineInfo)
            {
                SetSelection(machineInfo);
            }
            else
            {
                SetSelectionConveyor();
                Debug.LogWarning("Could not find machine " + save.machineName);
            }
        }
    }

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
    }
}
