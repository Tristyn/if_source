using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Machine))]
public class MachinePlacer : MonoBehaviour
{
    public ItemInfo itemInfo;
    public List<Conveyor> conveyors;

    Machine machine;
    int lastPlaceConveyorIndex;

    private void Awake()
    {
        machine = GetComponent<Machine>();
        if (machine)
        {
            itemInfo = machine.itemInfo;
        }
    }

    public bool PlaceItem()
    {
        List<Conveyor> conveyors = machine.conveyors;
        for (int i = lastPlaceConveyorIndex, len = conveyors.Count; i <= len; i++)
        {
            if (PlaceItem(conveyors[i]))
            {
                lastPlaceConveyorIndex = i;
                return true;
            }
        }
        for(int i = 0, len = lastPlaceConveyorIndex; i <= len; i++)
        {
            if (PlaceItem(conveyors[i]))
            {
                lastPlaceConveyorIndex = i;
                return true ;
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
