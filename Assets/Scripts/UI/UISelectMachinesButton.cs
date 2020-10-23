using UnityEngine;

public sealed class UISelectMachinesButton : MonoBehaviour
{
    public UISelectMachineButton current;
    public UISelectMachineButton machineButtonPrefab;
    public Vector3 entryOffset;

    public AudioClip expandListClip;

    UISelectMachineButton[] children;
    bool expanded;

    void Awake()
    {
        Init.Bind += OnBind;
    }

    void OnBind()
    {
        Vector3 groupOffset = Vector3.zero;

        MachineGroupInfo[] machineGroups = ScriptableObjects.instance.machineGroups;
        children = new UISelectMachineButton[ScriptableObjects.instance.machines.Length + 1]; // The magic number +1 is for conveyor
        int childrenIndex = 0;

        groupOffset.y += entryOffset.y;

        UISelectMachineButton conveyorButton = Instantiate(machineButtonPrefab, transform);
        conveyorButton.isConveyor = true;
        conveyorButton.transform.localPosition = groupOffset;
        conveyorButton.Initialize();
        children[childrenIndex] = conveyorButton;
        childrenIndex++;
        groupOffset.y += entryOffset.y;

        for (int i = 0, len = machineGroups.Length; i < len; ++i)
        {
            MachineGroupInfo machineGroup = machineGroups[i];
            for (int j = 0, lenJ = machineGroup.members.Length; j < lenJ; ++j)
            {
                UISelectMachineButton machineButton = Instantiate(machineButtonPrefab, transform);
                machineButton.machineInfo = machineGroup.members[j];
                machineButton.transform.localPosition = groupOffset;
                machineButton.Initialize();
                children[childrenIndex] = machineButton;
                childrenIndex++;
                groupOffset.x += entryOffset.x;
            }
            groupOffset.x = 0;
            groupOffset.y += entryOffset.y;
        }

        CollapseList();
    }

    public void ExpandList()
    {
        expanded = true;
        for (int i = 0, len = children.Length; i < len; ++i)
        {
            UISelectMachineButton machineButton = children[i];
            children[i].gameObject.SetActive(true);
        }
    }

    public void CollapseList()
    {
        expanded = false;
        for (int i = 0, len = children.Length; i < len; ++i)
        {
            children[i].gameObject.SetActive(false);
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
            current.PlaySelectMachineAudio();
            CollapseList();
        }
        else
        {
            PlayExpandListAudio();
            ExpandList();
        }
    }

    public void PlayExpandListAudio()
    {
        AudioSystem.instance.PlayOneShot(expandListClip, AudioCategory.Effect);
    }
}
