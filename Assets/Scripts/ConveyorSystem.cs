using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ConveyorSystem : Singleton<ConveyorSystem>
{
    public Dictionary<Vector3Int, Conveyor> conveyors = new Dictionary<Vector3Int, Conveyor>();
    public AudioClip createConveyorClip;
    public AudioClip demolishConveyorClip;

    public void Add(Conveyor conveyor)
    {
        Vector3Int position = conveyor.position_local;
        conveyors.Add(position, conveyor);
        Directions[] directions = EnumUtils<Directions>.nonZeroValues;

        // Set up neighbor pointers
        for(int i = 0, len = directions.Length; i < len; i++)
        {
            Directions direction = directions[i];
            if (conveyors.TryGetValue(position + direction.ToOffsetInt(), out Conveyor neighbor))
            {
                // Use ReferenceEquals to sidestep Unity overloading comparisons to null
                Assert.IsTrue(ReferenceEquals(conveyor.neighbors[(int)direction], null));
                Assert.IsTrue(ReferenceEquals(neighbor.neighbors[(int)direction.Inverse()], null));
                conveyor.neighbors[(int)direction] = neighbor;
                neighbor.neighbors[(int)direction.Inverse()] = conveyor;
            }
        }
    }

    public void Remove(Vector3Int position)
    {
        bool exists = conveyors.Remove(position);
        Assert.IsTrue(exists);
    }

    public bool CanCreate(Vector3Int position)
    {
        if (MachineSystem.instance.GetMachine(position, out Machine machine))
        {
            return false;
        }
        return true;
    }

    public bool CanLink(Vector3Int from, Vector3Int to)
    {
        if(MachineSystem.instance.GetMachine(to, out Machine toMachine))
        {
            if (!toMachine.canInput)
            {
                return false;
            }
        }
        if (MachineSystem.instance.GetMachine(from, out Machine fromMachine))
        {
            if (!fromMachine.canOutput)
            {
                return false;
            }
        }

        // Cannot link from machine to machine, or within the same machine
        if(toMachine && fromMachine)
        {
            return false;
        }
        return true;
    }
}
