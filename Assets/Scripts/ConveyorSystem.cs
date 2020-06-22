using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ConveyorSystem : Singleton<ConveyorSystem>
{
    public Dictionary<Vector3Int, Conveyor> conveyors = new Dictionary<Vector3Int, Conveyor>();

    public void Add(Conveyor conveyor)
    {
        Vector3Int position = conveyor.position;
        conveyors.Add(position, conveyor);
        Directions[] directions = EnumUtils<Directions>.nonZeroValues;

        // Set up neighbor pointers
        for(int i = 0, len = directions.Length; i < len; i++)
        {
            Directions direction = directions[i];
            if (conveyors.TryGetValue(position + direction.ToOffset(), out Conveyor neighbor))
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
}
