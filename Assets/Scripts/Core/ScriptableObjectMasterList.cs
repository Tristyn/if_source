using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class MachineGroup
{
    public MachineGroup(string groupName, float groupOrder)
    {
        this.groupName = groupName;
        this.groupOrder = groupOrder;
        members = Array.Empty<MachineInfo>();
        assemblers = Array.Empty<MachineInfo>();
        purchasers = Array.Empty<MachineInfo>();
        sellers = Array.Empty<MachineInfo>();
    }

    public string groupName;
    public float groupOrder;
    public MachineInfo[] members;
    public MachineInfo[] assemblers;
    public MachineInfo[] purchasers;
    public MachineInfo[] sellers;
}

[CreateAssetMenu(fileName = "ObjectMasterList", menuName = "Scriptable Object Master List", order = 32)]
public sealed class ScriptableObjectMasterList : ScriptableObject
{
    public ItemInfo[] items;
    public MachineInfo[] machines;
    public MachineGroup[] machineGroups;

    [NonSerialized]
    public Dictionary<string, ItemInfo> itemsDict;
    [NonSerialized]
    public Dictionary<string, MachineInfo> machinesDict;

    public void Initialize()
    {
        if (itemsDict == null)
        {
            itemsDict = new Dictionary<string, ItemInfo>(items.Length);
        }
        else
        {
            itemsDict.Clear();
        }
        for (int i = 0, len = items.Length; i < len; ++i)
        {
            ItemInfo itemInfo = items[i];
            itemsDict.Add(itemInfo.name, itemInfo);
        }

        if (machinesDict == null)
        {
            machinesDict = new Dictionary<string, MachineInfo>(machines.Length);
        }
        else
        {
            machinesDict.Clear();
        }
        for (int i = 0, len = machines.Length; i < len; ++i)
        {
            MachineInfo machineInfo = machines[i];
            machinesDict.Add(machineInfo.name, machineInfo);
        }
    }

    public void GroupMachines()
    {
        Dictionary<string, MachineGroup> groups = new Dictionary<string, MachineGroup>();
        foreach (MachineInfo machineInfo in machines)
        {
            if (!groups.TryGetValue(machineInfo.machineGroup, out MachineGroup group))
            {
                group = new MachineGroup(machineInfo.machineGroup, machineInfo.groupOrder);
            }

            if (machineInfo.assembler)
            {
                group.assemblers = group.assemblers.Append(machineInfo).ToArray();
            }
            if (machineInfo.purchaseItem.itemInfo != null)
            {
                group.purchasers = group.purchasers.Append(machineInfo).ToArray();
            }
            if (machineInfo.sellItem.itemInfo != null)
            {
                group.sellers = group.sellers.Append(machineInfo).ToArray();
            }

            if (!groups.ContainsKey(group.groupName))
            {
                groups.Add(group.groupName, group);
            }
        }

        machineGroups = groups.Values.OrderBy(machineInfo => machineInfo.groupOrder != 0 ? machineInfo.groupOrder : float.MaxValue).ToArray();

        for (int i = 0, len = machineGroups.Length; i < len; ++i)
        {
            machineGroups[i].members = machineGroups[i].assemblers.Concat(machineGroups[i].purchasers).Concat(machineGroups[i].sellers).ToArray();
        }
    }

    public void SetMachineGroupOrder(string groupName, float groupOrder)
    {
        for (int j = 0, jLen = machineGroups.Length; j < jLen; ++j)
        {
            MachineGroup group = machineGroups[j];
            if (group.groupName == groupName)
            {
                group.groupOrder = groupOrder;
                for (int i = 0, len = group.members.Length; i < len; ++i)
                {
                    group.members[i].groupOrder = groupOrder;
                }
            }
        }
    }
}