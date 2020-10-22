﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class MachinePlacer : MonoBehaviour, IFixedUpdate
{
    public Machine machine;

    Inventory inventory;
    ItemInfo itemInfo;

    const float placeInterval = 1 / Conveyor.itemSpeed;

    [Serializable]
    public struct Save
    {
        public float nextPlaceTime;
        public int lastOutputIndex;
    }

    [NonSerialized]
    public Save save;

    public void Initialize()
    {
        save.nextPlaceTime = GameTime.fixedTime;
        inventory = machine.inventory;
        if (machine.machinePurchaser)
        {
            itemInfo = machine.machineInfo.purchaseItem.itemInfo;
        }
        else if (machine.machineInfo.assembler)
        {
            itemInfo = machine.machineInfo.assembleOutput.itemInfo;
        }
        Updates.machinePlacers.Add(this);
    }

    public void Delete()
    {
        Updates.machinePlacers.Remove(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        if (save.nextPlaceTime <= GameTime.fixedTime)
        {
            save.nextPlaceTime += placeInterval;
            PlaceItem();
        }
    }

    void PlaceItem()
    {
        ref InventorySlot slot = ref inventory.GetSlot(itemInfo);
        if (slot.empty)
        {
            return;
        }

        MachineConveyorLink[] conveyorLinks = machine.conveyorLinks;
        int lastOutputIndex = Mathf.Min(save.lastOutputIndex, conveyorLinks.Length - 1);
        for (int i = lastOutputIndex + 1, len = conveyorLinks.Length; i < len; ++i)
        {
            MachineConveyorLink conveyorLink = conveyorLinks[i];
            if (conveyorLink.isOutput && conveyorLink.innerConveyor.PlaceItem(itemInfo, conveyorLink.machineDirection))
            {
                save.lastOutputIndex = i;
                --slot.count;
                if (slot.empty)
                {
                    return;
                }
            }
        }
        for (int i = 0, len = lastOutputIndex + 1; i < len; ++i)
        {
            MachineConveyorLink conveyorLink = conveyorLinks[i];
            if (conveyorLink.isOutput && conveyorLink.innerConveyor.PlaceItem(itemInfo, conveyorLink.machineDirection))
            {
                save.lastOutputIndex = i;
                --slot.count;
                if (slot.empty)
                {
                    return;
                }
            }
        }
        return;
    }
}