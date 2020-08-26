using System;
using System.Collections.Generic;
using UnityEngine;

public enum MachineGroupAchievementCategory : byte
{
    Unlock,
    Created,
    AssembleItem,
    SellItem,
    UpstreamSupplyChain,
    DemolishAll,
}

public sealed class MachineGroupAchievements : Singleton<MachineGroupAchievements>
{
    public Dictionary<MachineGroupInfo, bool[]> achievements = new Dictionary<MachineGroupInfo, bool[]>();
    public struct Save
    {
        public List<MachineGroupSave> machineGroups;
    }

    public struct MachineGroupSave
    {
        public string machineGroupName;
        public bool[] categories;
    }

    public void GetSave(out Save save)
    {
        List<MachineGroupSave> machineGroups = new List<MachineGroupSave>(achievements.Count);
        save.machineGroups = machineGroups;
        foreach (KeyValuePair<MachineGroupInfo, bool[]> category in achievements)
        {
            machineGroups.Add(new MachineGroupSave
            {
                machineGroupName = category.Key.machineGroupName,
                categories = category.Value
            });
        }
    }

    public void SetSave(in Save save)
    {
        int numCategories = EnumUtils<MachineGroupAchievementCategory>.valuesLength;
        achievements.Clear();
        if (save.machineGroups != null)
        {
            foreach (MachineGroupSave category in save.machineGroups)
            {
                bool[] categories = category.categories;
                if (numCategories != categories.Length)
                {
                    categories = new bool[numCategories];
                    Array.Copy(category.categories, categories, Mathf.Min(numCategories, categories.Length));
                }

                MachineGroupInfo machineGroup = ScriptableObjects.instance.GetMachineGroup(category.machineGroupName);
                achievements.Add(machineGroup, categories);
            }
        }
    }

    public void OnMachineUnlocked(MachineGroupInfo machineGroup)
    {
        Achieve(machineGroup, MachineGroupAchievementCategory.Unlock);
    }

    public void OnMachineCreated(MachineGroupInfo machineGroup)
    {
        Achieve(machineGroup, MachineGroupAchievementCategory.Created);
    }
    public void OnAssemblerLinkedToAssembler(Machine from, Machine to)
    {

    }

    public void OnMachineItemSold(MachineGroupInfo machineGroup)
    {
        Achieve(machineGroup, MachineGroupAchievementCategory.SellItem);
    }


    public void OnMachineDemolished(MachineGroupInfo machineGroup)
    {
        AchieveDemolishAll(machineGroup);
    }

    bool IsAchieved(MachineGroupInfo machineGroup, MachineGroupAchievementCategory machineGroupAchievementCategory)
    {
        if (achievements.TryGetValue(machineGroup, out bool[] categories))
        {
            return categories[(int)machineGroupAchievementCategory];
        }
        return false;
    }

    bool AchieveDemolishAll(MachineGroupInfo machineGroup)
    {
        if (!IsAchieved(machineGroup, MachineGroupAchievementCategory.DemolishAll))
        {
            // Only possible after AsUpstreamSupplyChain
            if (IsAchieved(machineGroup, MachineGroupAchievementCategory.SellItem)
                || IsAchieved(machineGroup, MachineGroupAchievementCategory.UpstreamSupplyChain))
            {
                MachineInfo[] groupMembers = machineGroup.members;
                for (int i = 0, len = groupMembers.Length; i < len; ++i)
                {
                    if (MachineSystem.instance.machinesMetaData.TryGetValue(groupMembers[i], out MachineMetaData machineMetaData)
                        && machineMetaData.numInstances > 0)
                    {
                        return false;
                    }
                }
                return Achieve(machineGroup, MachineGroupAchievementCategory.DemolishAll);
            }
        }
        return false;
    }

    bool Achieve(MachineGroupInfo machineGroup, MachineGroupAchievementCategory machineGroupAchievementCategory)
    {
        if (!achievements.TryGetValue(machineGroup, out bool[] categories))
        {
            categories = new bool[EnumUtils<MachineGroupAchievementCategory>.valuesLength];
            achievements.Add(machineGroup, categories);
        }
        if (!categories[(int)machineGroupAchievementCategory])
        {
            categories[(int)machineGroupAchievementCategory] = true;
            Analytics.instance.NewMachineGroupProgressionEvent(machineGroup, machineGroupAchievementCategory);
            return true;
        }
        return false;
    }
}
