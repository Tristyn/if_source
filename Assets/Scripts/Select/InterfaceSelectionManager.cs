using UnityEngine;

public sealed class InterfaceSelectionManager : Singleton<InterfaceSelectionManager>
{
    public SelectionState state = new SelectionState();

    public struct Save
    {
        public SelectionMode selectionMode;
        public string machineName;
    }

    public void GetSave(out Save save)
    {
        save.selectionMode = state.selectionMode;
        save.machineName = state.machineInfo ? state.machineInfo.machineName : null;
    }

    public void SetSave(in Save save)
    {
        if (save.selectionMode == SelectionMode.Conveyor)
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
        state = new SelectionState
        {
            selectionMode = SelectionMode.Machine,
            machineInfo = machineInfo
        };

        Events.InterfaceSelectionChanged?.Invoke(state);
    }

    public void SetSelectionConveyor()
    {
        state = new SelectionState
        {
            selectionMode = SelectionMode.Conveyor
        };
        Events.InterfaceSelectionChanged?.Invoke(state);
    }

    public void SetSelectionNone()
    {
        state = new SelectionState
        {
            selectionMode = SelectionMode.None
        };
        Events.InterfaceSelectionChanged?.Invoke(state);
    }
}
