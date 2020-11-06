using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MachineAnalysis
{
    public static MachineInfo GetItemPurchaser(this MachineInfo[] machineInfos, ItemInfo itemInfo)
    {
        for (int i = 0, len = machineInfos.Length; i < len; ++i)
        {
            if (machineInfos[i].purchaseItem.itemInfo == itemInfo)
            {
                return machineInfos[i];
            }
        }
        return null;
    }

    public static MachineInfo GetItemAssembler(this MachineInfo[] machineInfos, ItemInfo itemInfo)
    {
        for (int i = 0, len = machineInfos.Length; i < len; ++i)
        {
            if (machineInfos[i].assembleOutput.itemInfo == itemInfo)
            {
                return machineInfos[i];
            }
        }
        return null;
    }

    public static MachineInfo GetItemSeller(this MachineInfo[] machineInfos, ItemInfo itemInfo)
    {
        for (int i = 0, len = machineInfos.Length; i < len; ++i)
        {
            if (machineInfos[i].sellItem.itemInfo == itemInfo)
            {
                return machineInfos[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Calculates the cost to purchase or assembling one item.
    /// </summary>
    public static float CalculateExpenses(MachineInfo machineInfo, MachineInfo[] machineInfos, HashSet<MachineInfo> visitedMachines)
    {
        if (visitedMachines.Contains(machineInfo))
        {
            return 0f;
        }
        visitedMachines.Add(machineInfo);

        if (machineInfo.purchaseItem.itemInfo)
        {
            return machineInfo.purchaseItem.itemInfo.value / machineInfo.placeInterval;
        }
        else if (machineInfo.sellItem.itemInfo)
        {
            MachineInfo assembleMachine = machineInfos.GetItemAssembler(machineInfo.sellItem.itemInfo);
            if (assembleMachine)
            {
                return CalculateExpenses(assembleMachine, machineInfos, visitedMachines);
            }
        }
        else if (machineInfo.assembler)
        {
            float expenses = 0;
            for (int i = 0, len = machineInfo.assembleInputs.Length; i < len; ++i)
            {
                AssembleSlot assembleInput = machineInfo.assembleInputs[i];
                MachineInfo outputMachine = machineInfos.GetItemAssembler(assembleInput.itemInfo);
                if (!outputMachine)
                {
                    outputMachine = machineInfos.GetItemPurchaser(assembleInput.itemInfo);
                }

                if (assembleInput.itemInfo && outputMachine)
                {
                    expenses += CalculateExpenses(outputMachine, machineInfos, visitedMachines) * assembleInput.count;
                }
            }
            return expenses / machineInfo.assembleOutput.count;
        }

        return 0f;
    }

    /// <summary>
    /// Calculates the profit of producing or assembling one item.
    /// </summary>
    public static float CalculateProfit(MachineInfo machineInfo, MachineInfo[] machineInfos, HashSet<MachineInfo> visitedMachines)
    {
        ItemInfo itemInfo = null;
        if (machineInfo.assembler)
        {
            itemInfo = machineInfo.assembleOutput.itemInfo;
        }
        else if (machineInfo.sellItem.itemInfo)
        {
            itemInfo = machineInfo.sellItem.itemInfo;
        }

        if (itemInfo)
        {
            return itemInfo.value - CalculateExpenses(machineInfo, machineInfos, visitedMachines);
        }
        else
        {
            return 0f;
        }
    }


    /// <summary>
    /// Calculates the profit of producing or assembling one item for all machines.
    /// </summary>
    public static void CalculateProfit(MachineInfo[] machineInfos, HashSet<MachineInfo> visitedMachines)
    {
        for (int i = 0, len = machineInfos.Length; i < len; ++i)
        {
            visitedMachines.Clear();
            machineInfos[i].profit = CalculateProfit(machineInfos[i], machineInfos, visitedMachines);
        }
    }

    /// <summary>
    /// Marks target objects as dirty
    /// </summary>
    /// <param name="scriptableObjects"></param>
    public static void SetDirty(this Object[] objects)
    {
#if UNITY_EDITOR
        for (int i = 0, len = objects.Length; i < len; ++i)
        {
            EditorUtility.SetDirty(objects[i]);
        }
#endif
    }
}
