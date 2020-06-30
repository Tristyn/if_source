using UnityEngine;

public class UISelectMachinesButton : MonoBehaviour
{
    public ScriptableObjectMasterList masterList;
    public UISelectMachineButton current;
    public UISelectMachineButton machineButtonPrefab;
    public Vector3 entryOffset;

    GameObject[] children;
    bool expanded;

    void Awake()
    {
        Vector3 groupOffset = Vector3.zero;

        MachineGroup[] machineGroups = masterList.machineGroups;
        children = new GameObject[masterList.allMachines.Length + 1]; // The magic number +1 is for conveyor
        int childrenIndex = -1;
        for (int i = 0, len = machineGroups.Length; i < len; i++)
        {
            groupOffset.y += entryOffset.y;

            UISelectMachineButton conveyorButton = Instantiate(machineButtonPrefab, transform);
            conveyorButton.isConveyor = true;
            conveyorButton.transform.localPosition = groupOffset;
            conveyorButton.Initialize();
            children[++childrenIndex] = conveyorButton.gameObject;
            groupOffset.y += entryOffset.y;

            MachineGroup machineGroup = machineGroups[i];
            for(int j = 0, lenJ = machineGroup.members.Length; j < lenJ; j++)
            {
                UISelectMachineButton machineButton = Instantiate(machineButtonPrefab, transform);
                machineButton.machineInfo = machineGroup.members[j];
                machineButton.transform.localPosition = groupOffset;
                machineButton.Initialize();
                children[++childrenIndex] = machineButton.gameObject;
                groupOffset.x += entryOffset.x;
            }
        }

        CollapseList();
    }

    public void ExpandList()
    {
        expanded = true;
        for (int i = 0, len = children.Length; i < len; i++)
        {
            children[i].SetActive(true);
        }
    }

    public void CollapseList()
    {
        expanded = false;
        for (int i = 0, len = children.Length; i < len; i++)
        {
            children[i].SetActive(false);
        }
    }

    public void OnSelectionChanged()
    {
        InterfaceState selectionState = InterfaceSelectionManager.instance.state;
        current.isConveyor = selectionState.mode == InterfaceMode.Conveyor;
        current.machineInfo = selectionState.machineInfo;
        current.Initialize();
    }

    public void OnCurrentClicked()
    {
        if (expanded)
        {
            CollapseList();
        }
        else
        {
            ExpandList();
        }
    }
}
