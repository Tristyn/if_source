using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public struct ConveyorItem
{
    Transform itemTransform;
    float distance;
    int frameOfLastMove;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Item GetItem()
    {
        return itemTransform.GetComponent<Item>();
    }
}

public class Conveyor : MonoBehaviour
{
    public Vector3Int position;
    [NonSerialized]
    public Conveyor[] neighbors = new Conveyor[EnumUtils<Directions>.values.Length];
    [NonSerialized]
    public Conveyor[] inputs = new Conveyor[EnumUtils<Directions>.values.Length];
    [NonSerialized]
    public Conveyor[] outputs = new Conveyor[EnumUtils<Directions>.values.Length];
    [NonSerialized]
    public ConveyorLink[] outputLinks = new ConveyorLink[EnumUtils<Directions>.values.Length];
    [NonSerialized]
    public Queue<ConveyorItem>[] items = new Queue<ConveyorItem>[EnumUtils<Directions>.values.Length];
    [NonSerialized]
    public Machine machine;

    public static float itemDistance;
    int lastOutputIndex;

    public static Conveyor CreateConveyor(Vector3Int position)
    {
        if (ConveyorSystem.instance.conveyors.TryGetValue(position, out Conveyor conveyor))
        {
            return conveyor;
        }

        conveyor = ObjectPooler.instance.Get<Conveyor>();
        conveyor.transform.position = position;
        conveyor.Initialize();
        return conveyor;
    }

    public static Conveyor CreateConveyor(Vector3Int fromTile, Vector3Int toTile)
    {
        if ((fromTile - toTile).ToDirection() == Directions.None)
        {
            Debug.LogWarning("Can't link conveyors that aren't neighbors.");
            return null;
        }

        if (!ConveyorSystem.instance.conveyors.TryGetValue(toTile, out Conveyor conveyor))
        {
            conveyor = CreateConveyor(toTile);
        }
        if (!ConveyorSystem.instance.conveyors.TryGetValue(fromTile, out Conveyor sourceConveyor))
        {
            sourceConveyor = CreateConveyor(fromTile);
        }
        if (conveyor && sourceConveyor)
        {
            sourceConveyor.Link(conveyor);
        }
        return conveyor;
    }

    public void Initialize()
    {
        position = transform.position.RoundToTile();
        ConveyorSystem.instance.Add(this);
        if (MachineSystem.instance.GetMachine(position, out Machine machine))
        {
            machine.AddConveyor(this);
        }
    }

    public void Recycle()
    {
        if (machine)
        {
            machine.RemoveConveyor(this);
        }

        Directions[] directions = EnumUtils<Directions>.nonZeroValues;

        // Remove neighbors
        for (int i = 0, len = directions.Length; i < len; i++)
        {
            Directions direction = directions[i];
            Conveyor neighbor = neighbors[(int)direction];
            if (neighbor)
            {
                // Use ReferenceEquals to sidestep Unity overloading comparisons to null
                Assert.IsTrue(ReferenceEquals(neighbors[(int)direction], neighbor));
                Assert.IsTrue(ReferenceEquals(neighbor.neighbors[(int)direction.Inverse()], this));

                Directions inverseDirection = direction.Inverse();
                Conveyor output = outputs[(int)direction];
                Conveyor input = neighbor.outputs[(int)inverseDirection];
                Assert.IsFalse(input && output);
                if (output)
                {
                    Unlink(neighbor);
                }
                if (input)
                {
                    neighbor.Unlink(this);
                }

                neighbors[(int)direction] = null;
                neighbor.neighbors[(int)inverseDirection] = null;
            }
        }

        ConveyorSystem.instance.Remove(position);
        ObjectPooler.instance.Recycle(this);
    }

    public void Link(Conveyor to)
    {
        Directions direction = (to.position - position).ToDirection();
        if (direction != Directions.None && !outputs[(int)direction])
        {
            to.Unlink(this);

            outputs[(int)direction] = to;
            inputs[(int)direction] = null;
            Directions inverseDirection = direction.Inverse();
            to.inputs[(int)inverseDirection] = this;
            to.outputs[(int)inverseDirection] = null;

            ConveyorLink link = ObjectPooler.instance.Get<ConveyorLink>();
            outputLinks[(int)direction] = link;
            link.transform.SetParent(transform);
            link.transform.position = transform.position;
            link.direction = direction;
            link.Initialize();
        }
    }

    public void Unlink(Conveyor to)
    {
        Directions direction = (to.position - position).ToDirection();
        if (direction != Directions.None && outputs[(int)direction])
        {
            Directions inverseDirection = direction.Inverse();
            Assert.IsNull(inputs[(int)direction]);
            Assert.IsNull(to.outputs[(int)inverseDirection]);
            outputs[(int)direction] = null;
            to.inputs[(int)inverseDirection] = null;

            outputLinks[(int)direction].Recycle();
            outputLinks[(int)direction] = null;
        }
    }

    public bool PlaceItem(ItemInfo itemInfo)
    {
        for (int i = lastOutputIndex, len = outputs.Length; i <= len; i++)
        {
            if (PlaceItem(i, itemInfo))
            {
                lastOutputIndex = i;
                return true;
            }
        }
        for (int i = 1, len = lastOutputIndex; i <= len; i++)
        {
            if (PlaceItem(i, itemInfo))
            {
                lastOutputIndex = i;
                return true;
            }
        }
        Item item = ItemPooler.instance.Get(itemInfo);

        return false;
    }

    bool PlaceItem(int outputIndex, ItemInfo itemInfo)
    {
        return false;
        //Conveyor conveyor = outputs[outputIndex];
        //if (conveyor)
        //{
        //    Queue<ConveyorItem> queue = null;
            
        //    ConveyorItem[] conveyorItems = items[outputIndex];
        //    if (PlaceItem(conveyors[i]))
        //    {
        //        lastPlaceConveyorIndex = i;
        //        return true;
        //    }
        //}
    }
}
