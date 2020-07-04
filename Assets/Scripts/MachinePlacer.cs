using UnityEngine;

public class MachinePlacer : MonoBehaviour
{
    public Machine machine;

    Inventory inventory;
    ItemInfo itemInfo;
    int lastOutputIndex;

    const float placeInterval = 1 / Conveyor.itemSpeed;
    float nextPlaceTime = -1f;

    public void Initialize()
    {
        inventory = machine.inventory;
        if (machine.machinePurchaser)
        {
            itemInfo = machine.machineInfo.purchaseItem.itemInfo;
        }
        else if (machine.machineInfo.assembler)
        {
            itemInfo = machine.machineInfo.assembleOutput.itemInfo;
        }
    }

    void FixedUpdate()
    {
        if (nextPlaceTime <= Time.fixedTime)
        {
            nextPlaceTime += placeInterval;
            if (inventory.HasItem(itemInfo))
            {
                if (PlaceItem())
                {
                    inventory.DeductItem(itemInfo);
                }
            }
        }
    }

    bool PlaceItem()
    {
        MachineConveyorLink[] conveyorLinks = machine.conveyorLinks;
        int lastOutputIndex = Mathf.Min(this.lastOutputIndex, conveyorLinks.Length - 1);
        for (int i = lastOutputIndex + 1, len = conveyorLinks.Length; i < len; i++)
        {
            MachineConveyorLink conveyorLink = conveyorLinks[i];
            if (conveyorLink.isOutput && conveyorLink.innerConveyor.PlaceItem(itemInfo, conveyorLink.direction))
            {
                this.lastOutputIndex = i;
                return true;
            }
        }
        for (int i = 0, len = lastOutputIndex + 1; i < len; i++)
        {
            MachineConveyorLink conveyorLink = conveyorLinks[i];
            if (conveyorLink.isOutput && conveyorLink.innerConveyor.PlaceItem(itemInfo, conveyorLink.direction))
            {
                this.lastOutputIndex = i;
                return true;
            }
        }
        return false;
    }
}
