using System;
using UnityEngine;

public class MachinePlacer : MonoBehaviour
{
    public Machine machine;

    ItemInfo itemInfo;
    int lastOutputIndex;

    public void Initialize()
    {
        if (machine.machinePurchaser)
        {
            itemInfo = machine.machineInfo.purchaseItem;
        }
        else if (machine.machineInfo.assembler)
        {
            itemInfo = machine.machineInfo.assembleOutput.itemInfo;
        }
    }

    public bool PlaceItem()
    {
        MachineConveyorLink[] conveyorLinks = machine.conveyorLinks;
        int lastOutputIndex = Mathf.Min(this.lastOutputIndex, conveyorLinks.Length-1);
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
