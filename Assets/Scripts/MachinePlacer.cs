using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Machine))]
public class MachinePlacer : MonoBehaviour
{
    public Machine machine;
    ItemInfo itemInfo;
    int lastPlaceConveyorIndex;

    public void Initialize()
    {
        itemInfo = machine.itemInfo;
    }

    public bool PlaceItem()
    {
        Conveyor[] conveyors = machine.conveyors;
        for (int i = lastPlaceConveyorIndex + 1, len = conveyors.Length; i < len; i++)
        {
            Conveyor conveyor = conveyors[i];
            if (PlaceItem(conveyor))
            {
                lastPlaceConveyorIndex = i;
                return true;
            }
        }
        for (int i = 0, len = lastPlaceConveyorIndex; i <= len; i++)
        {
            Conveyor conveyor = conveyors[i];
            if (PlaceItem(conveyor))
            {
                lastPlaceConveyorIndex = i;
                return true;
            }
        }
        return false;
    }

    bool PlaceItem(Conveyor conveyor)
    {
        if (conveyor && conveyor.PlaceItem(itemInfo))
        {
            return true;
        }
        return false;
    }
}
