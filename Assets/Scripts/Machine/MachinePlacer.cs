using System;
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
        Entities.machinePlacers.Add(this);
    }

    public void Delete()
    {
        Entities.machinePlacers.Remove(this);
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
        int start = Mathf.Min(save.lastOutputIndex + 1, conveyorLinks.Length);
        for (int i = start, iter = 0, len = conveyorLinks.Length; iter < len; ++i, ++iter)
        {
            if (i == len)
            {
                i = 0;
            }
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
    }
}
