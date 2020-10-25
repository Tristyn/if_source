using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectMasterList", menuName = "Scriptable Object Master List", order = 32)]
public sealed class ScriptableObjectMasterList : ScriptableObject
{
    public ItemInfo[] items;
    public MachineInfo[] machines;
    public MachineVisual machineVisualDefault;
    public MachineGroupInfo[] machineGroups;
    public ProgressionInfo[] progressionInfos;
    [ReadOnly]
    public int nextProgressionId;

    [NonSerialized]
    public Dictionary<string, ItemInfo> itemsDict;
    [NonSerialized]
    public Dictionary<string, MachineInfo> machinesDict;
    [NonSerialized]
    public Dictionary<string, MachineGroupInfo> machineGroupsDict;

#if UNITY_EDITOR
    public void OnValidate()
    {
        items = items.Where(item => item).ToArray();
        machines = machines.Where(machine => machine).ToArray();
        machineGroups = machineGroups.Where(machineGroup => machineGroup).ToArray();
        progressionInfos = progressionInfos.Where(progressionInfo => progressionInfo).ToArray();
        EditorUtility.SetDirty(this);
    }
#endif

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
            machineInfo.Initialize();
            machinesDict.Add(machineInfo.name, machineInfo);
        }

        if (machineGroupsDict == null)
        {
            machineGroupsDict = new Dictionary<string, MachineGroupInfo>();
        }
        else
        {
            machineGroupsDict.Clear();
        }
        for (int i = 0, len = machineGroups.Length; i < len; ++i)
        {
            MachineGroupInfo machineGroup = machineGroups[i];
            machineGroupsDict.Add(machineGroup.machineGroupName, machineGroup);
        }
    }

    public ItemInfo GetItemInfo(string name)
    {
        if (itemsDict.TryGetValue(name, out ItemInfo itemInfo))
        {
            return itemInfo;
        }
        return null;
    }

    public MachineInfo GetMachineInfo(string name)
    {
        if (machinesDict.TryGetValue(name, out MachineInfo machineInfo))
        {
            return machineInfo;
        }
        return null;
    }

    public MachineGroupInfo GetMachineGroup(string name)
    {
        if (machineGroupsDict.TryGetValue(name, out MachineGroupInfo machineGroup))
        {
            return machineGroup;
        }
        return null;
    }

#if UNITY_EDITOR
    public static ScriptableObjectMasterList LoadAsset()
    {
        var masterList = AssetDatabase.LoadAssetAtPath<ScriptableObjectMasterList>(
            AssetDatabase.FindAssets("ObjectMasterList t:ScriptableObjectMasterList", new[] { "Assets/Resources" })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .SingleOrDefault());
        if (masterList)
        {
            masterList.OnValidate();
        }
        return masterList;
    }
#endif
}