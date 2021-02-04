using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Conveyor : MonoBehaviour, IFixedUpdate
{
    public Machine machine;
    [NonSerialized]
    public Inventory machineInventory;

    public ConveyorSegment[] conveyorSegments = new ConveyorSegment[EnumUtil<Directions>.valuesLength];
    public ConveyorLink[] outputLinks = new ConveyorLink[EnumUtil<Directions>.valuesLength];
    public Conveyor[] outputConveyors = new Conveyor[EnumUtil<Directions>.valuesLength];

    public const float minItemDistance = 0.5000001f;
    public const float queueDistance = 1f;
    public const float itemSpeed = 2f;
    public const float fixedItemSpeed = itemSpeed / GameTime.fixedFrameRate;
    public const float throughput = itemSpeed / minItemDistance;

    public Save save;

    public struct Save
    {
        public Vector3Int position_local;
        public DirectionsFlag outputs;
        public DirectionsFlag inputs;
        public bool hasRouterInput;
        public Directions currentRouterInput;
        public Directions lastRoutedDirection;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ConveyorSegment.Save[] outputQueues;
    }

    public void Initialize()
    {
        transform.localPosition = save.position_local.RoundToTileCenter();
        ConveyorSystem.instance.conveyors.Add(save.position_local, this);
        Machine machine = MachineSystem.instance.GetMachine(save.position_local);
        if (machine)
        {
            LinkMachine(machine);
            machine.FindConveyors();
        }
        Entities.conveyors.Add(this);
    }

    public void Recycle()
    {
        save.hasRouterInput = false;

        // Remove neighbors
        Directions[] directions = EnumUtil<Directions>.values;
        for (int i = 0, len = directions.Length; i < len; ++i)
        {
            Directions direction = directions[i];
            Assert.IsFalse(save.outputs.Get(direction) && save.inputs.Get(direction));
            Conveyor neighbor = null;
            if (save.outputs.Get(direction))
            {
                neighbor = TryGetNeighbor(direction);
                Unlink(neighbor);
            }
            else if (save.inputs.Get(direction))
            {
                neighbor = TryGetNeighbor(direction);
                neighbor.Unlink(this);
            }
            if (neighbor && neighbor.machine)
            {
                neighbor.machine.RecycleInvalidConveyors();
            }
        }

        bool exists = ConveyorSystem.instance.conveyors.Remove(save.position_local);
        Assert.IsTrue(exists);
        if (machine)
        {
            UnlinkMachine();
        }
        Entities.conveyors.Remove(this);
        ObjectPooler.instance.Recycle(this);
    }

    public void GetSave(out Save save)
    {
        if (this.save.outputs.Any())
        {
            Directions[] directions = EnumUtil<Directions>.values;
            int lenDirections = directions.Length;
            ConveyorSegment.Save[] saveOutputQueues = new ConveyorSegment.Save[lenDirections];
            this.save.outputQueues = saveOutputQueues;
            for (int i = 0; i < lenDirections; ++i)
            {
                ConveyorSegment conveyorSegment = conveyorSegments[i];
                if (conveyorSegment.valid)
                {
                    conveyorSegment.GetSave(out saveOutputQueues[i]);
                }
            }
        }
        else
        {
            save.outputQueues = Array.Empty<ConveyorSegment.Save>();
        }
        save = this.save;
    }

    public void SetSave(in Save save)
    {
        // At this point all conveyors are place before the first SetSave
        this.save = save;
        DirectionsFlag saveOutputs = save.outputs;
        if (saveOutputs.Any())
        {
            ConveyorSegment.Save[] saveOutputQueues = save.outputQueues;

            int len = saveOutputQueues.Length;
            for (int i = 0; i < len; ++i)
            {
                if (saveOutputs.Get((Directions)i))
                {
                    Conveyor neighbor = TryGetNeighbor((Directions)i);
                    if (neighbor)
                    {
                        DoLink(neighbor, (Directions)i);

                        conveyorSegments[i].SetSave(in save.outputQueues[i]);
                    }
                    else
                    {
                        Debug.LogWarning("Failed to find conveyor neighbor when loading save.", this);
                    }
                }
            }
        }
    }

    public void Demolish()
    {
        ConveyorSystem.instance.PlayDemolishAudio();
        CurrencySystem.instance.SellConveyor();
        for (int i = 0, len = conveyorSegments.Length; i < len; ++i)
        {
            if (conveyorSegments[i].valid)
            {
                conveyorSegments[i].hasItems = false;
            }
        }
        Recycle();
    }

    public Conveyor TryGetNeighbor(Directions direction)
    {
        Vector3Int neighborPosition = save.position_local + direction.ToOffsetInt();
        ConveyorSystem.instance.conveyors.TryGetValue(neighborPosition, out Conveyor neighbor);
        return neighbor;
    }

    public Conveyor TryGetInput(Directions direction)
    {
        if (save.inputs.Get(direction))
        {
            Vector3Int neighborPosition = save.position_local + direction.ToOffsetInt();
            return ConveyorSystem.instance.conveyors[neighborPosition];
        }
        return null;
    }

    public Conveyor TryGetOutput(Directions direction)
    {
        Vector3Int neighborPosition = save.position_local + direction.ToOffsetInt();
        if (save.outputs.Get(direction))
        {
            return ConveyorSystem.instance.conveyors[neighborPosition];
        }
        return null;
    }

    public void Link(Conveyor to)
    {
        (bool isNeighbor, Directions direction) = save.position_local.ToDirection(to.save.position_local);
        if (isNeighbor && !IsLinked(to))
        {
            DoLink(to, direction);
        }
    }

    void DoLink(Conveyor to, Directions direction)
    {
        to.Unlink(this);

        Directions inverseDirection = direction.Inverse();
        save.outputs.SetTrue(direction);
        outputConveyors[(int)direction] = to;
        conveyorSegments[(int)direction].queue = new OpenQueue<ConveyorItem>();
        to.save.inputs.SetTrue(inverseDirection);

        Assert.IsTrue(save.outputs.Get(direction));
        Assert.IsTrue(to.save.inputs.Get(inverseDirection));
        Assert.IsFalse(save.inputs.Get(direction));
        Assert.IsFalse(to.save.outputs.Get(inverseDirection));

        ConveyorLink link = ObjectPooler.instance.Get<ConveyorLink>();
        outputLinks[(int)direction] = link;
        link.transform.SetParent(transform, worldPositionStays: false);
        Assert.IsTrue(link.transform.localPosition == Vector3.zero);
        link.direction = direction;
        link.Initialize();

        if (machine)
        {
            machine.FindConveyors();
        }
        if (to.machine)
        {
            to.machine.FindConveyors();
        }
    }

    public void Unlink(Conveyor to)
    {
        (bool isNeighbor, Directions direction) = save.position_local.ToDirection(to.save.position_local);
        if (isNeighbor && save.outputs.Get(direction))
        {
            DoUnlink(to, direction);
        }
    }

    void DoUnlink(Conveyor to, Directions direction)
    {
        Directions inverseDirection = direction.Inverse();
        Assert.IsTrue(save.outputs.Get(direction));
        Assert.IsTrue(to.save.inputs.Get(inverseDirection));
        Assert.IsFalse(save.inputs.Get(direction));
        Assert.IsFalse(to.save.outputs.Get(inverseDirection));

        conveyorSegments[(int)direction].hasItems = false;
        conveyorSegments[(int)direction].queue = null;
        save.outputs.SetFalse(direction);
        to.save.inputs.SetFalse(inverseDirection);
        outputConveyors[(int)direction] = null;

        Assert.IsFalse(save.outputs.Get(direction));
        Assert.IsFalse(to.save.inputs.Get(inverseDirection));

        outputLinks[(int)direction].Recycle();
        outputLinks[(int)direction] = null;

        if (to.save.currentRouterInput == direction)
        {
            to.save.hasRouterInput = false;
        }

        if (machine)
        {
            machine.FindConveyors();
        }
        if (to.machine)
        {
            to.machine.FindConveyors();
        }
    }

    public bool HasAnyLinks()
    {
        Directions[] directions = EnumUtil<Directions>.values;
        for (int i = 0, len = directions.Length; i < len; ++i)
        {
            Directions direction = directions[i];
            if (save.outputs.Get(direction) || save.inputs.Get(direction))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsLinked(Conveyor to)
    {
        Assert.IsNotNull(to);
        (bool isNeighbor, Directions direction) = save.position_local.ToDirection(to.save.position_local);
        if (isNeighbor && save.outputs.Get(direction))
        {
            return true;
        }
        return false;
    }

    public void LinkMachine(Machine machine)
    {
        Debug.Assert(!this.machine || this.machine == machine);
        Debug.Assert(machine.bounds.Contains(save.position_local));
        this.machine = machine;
        machineInventory = machine.inventory;
    }

    void UnlinkMachine()
    {
        Machine machine = this.machine;
        Assert.IsNotNull(machine);
        Assert.IsTrue(machineInventory.valid);
        this.machine = null;
        machineInventory = default;
        machine.FindConveyors();
    }

    public bool PlaceItem(ItemInfo itemInfo, Directions direction)
    {
        // it is intentional to not check distance of other queues because items are only placed
        // inside machines and there we only care about the singular machine output.
        // When items transfer between conveyor queues we check distance on every queue.
        Assert.IsTrue(save.outputs.Get(direction));
        return conveyorSegments[(int)direction].PlaceItem(itemInfo);
    }

    /// <summary>
    /// Find the next route direction in the sequence
    /// </summary>
    public OpenQueue<ConveyorItem> GetRouteDirection(out Directions direction)
    {

        for (int i = (int)save.lastRoutedDirection + 1, iter = 0, len = conveyorSegments.Length; iter < len; ++i, ++iter)
        {
            if (i == len)
            {
                i = 0;
            }
            OpenQueue<ConveyorItem> queue = conveyorSegments[i].queue;
            if (queue != null)
            {
                direction = (Directions)i;
                return queue;
            }
        }

        direction = default;
        return null;
    }

    /// <summary>
    /// Find the next route direction in the sequence that has space to insert another item
    /// </summary>
    bool GetRouteDirection(out Directions direction, out OpenQueue<ConveyorItem> destination, out float routedItemDistance)
    {
        // These 2 loops checks every output queue to find the destination queue,
        // and also make sure there is some space on every queue
        for (int i = (int)save.lastRoutedDirection + 1, iter = 0, len = conveyorSegments.Length; iter < len; ++i, ++iter)
        {
            if (i == len)
            {
                i = 0;
            }

            if (!save.outputs.Get((Directions)i))
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = conveyorSegments[i].queue;
            Assert.IsNotNull(queue);
            ConveyorItem[] queueArray = queue.array;
            if (queue.Count == 0)
            {
                routedItemDistance = queueDistance;
                direction = (Directions)i;
                destination = queue;
                return true;
            }
            else if (queueArray[queue.tail].distance - minItemDistance < 0)
            {
                continue;
            }

            routedItemDistance = queueArray[queue.tail].distance;
            direction = (Directions)i;
            destination = queue;
            return true;
        }

        direction = default;
        destination = null;
        routedItemDistance = queueDistance;
        return false;
    }

    /// <summary>
    /// Finds the closest item that has passed the router and returns its distance in it's current queue.
    /// </summary>
    float RoutedItemDistance()
    {
        float distance = queueDistance;
        if (machineInventory.valid)
        {
            return distance;
        }
        for (int i = 0, len = conveyorSegments.Length; i < len; ++i)
        {
            OpenQueue<ConveyorItem> queue = conveyorSegments[i].queue;
            if (queue == null || queue.Count == 0)
            {
                continue;
            }
            distance = Mathf.Min(distance, queue.array[queue.tail].distance);
        }
        return distance;
    }

    public bool CanTransferIn(Directions outputDirection)
    {
        if (machineInventory.valid || !save.hasRouterInput || save.currentRouterInput == outputDirection)
        {
            return true;
        }
        return false;
    }

    public void BeginTransferIn(Directions outputDirection)
    {
        save.hasRouterInput = true;
        save.currentRouterInput = outputDirection;
    }

    public bool EndTransferIn(in ConveyorItem conveyorItem, ref float transferredDistance)
    {
        Assert.IsTrue(transferredDistance >= 0f);
        if (machineInventory.valid)
        {
            Item item = conveyorItem.GetItem();
            ItemInfo itemInfo = item.itemInfo;
            ref InventorySlot slot = ref machineInventory.GetSlot(itemInfo, InventorySlotType.Input);
            if (slot.valid && slot.TryIncrement())
            {
                item.ConsumedByMachine();
                return true;
            }
            return false;
        }
        else if (GetRouteDirection(out Directions direction, out OpenQueue<ConveyorItem> destination, out float routedItemDistance) && routedItemDistance >= minItemDistance)
        {
            save.lastRoutedDirection = direction;
            save.hasRouterInput = false;

            float distance = Mathf.Min(transferredDistance, routedItemDistance - minItemDistance);
            transferredDistance = distance;

            ConveyorItem transferItem = conveyorItem;
            transferItem.distance = distance;
            transferItem.UpdateTransform(transform.localPosition, direction);
            destination.Enqueue(transferItem);
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        /* Updating a conveyor queue updates 4 types of items
         * - The outgoing item transitioning to the machine, the router or another conveyor
         * - Other outgoing items
         * - The incoming item entering the machine or the router, the head item
         * - Other incoming items
         * each item relies on the lookAheadItemDistance from the previous item so 
         * it's all done in one monolithic update.
         */

        DirectionsFlag outputFlags = this.save.outputs;
        DirectionsFlag[] directionsFlags = EnumUtil<DirectionsFlag>.values;
        Vector3 position_local = transform.localPosition;
        for (int i = 0, len = directionsFlags.Length; i < len; ++i)
        {
            if (outputFlags.Get(directionsFlags[i]))
            {
                OpenQueue<ConveyorItem> queue = conveyorSegments[i].queue;
                Assert.IsNotNull(queue);
                if (queue.Count == 0)
                {
                    continue;
                }
                ConveyorItem[] queueArray = queue.array;
                int queueHead = queue.head;
                ref ConveyorItem head = ref queueArray[queueHead];
                float headDistance = head.distance;
                if (headDistance + fixedItemSpeed < queueDistance - minItemDistance)
                {
                    headDistance += fixedItemSpeed;
                    head.distance = headDistance;
                    head.UpdateTransform(transform.localPosition, (Directions)i);
                }
                else
                {
                    Conveyor outputConveyor = outputConveyors[i];
                    if (outputConveyor.CanTransferIn((Directions)i))
                    {
                        outputConveyor.BeginTransferIn((Directions)i);
                        headDistance += fixedItemSpeed;
                        if (headDistance >= queueDistance)
                        {
                            float transferredHeadDistance = headDistance - queueDistance;
                            // We only transfer when all queues have enough space
                            // That's why it doesn't need to check for space on each route once it's transferred and routed
                            if (outputConveyor.EndTransferIn(in head, ref transferredHeadDistance))
                            {
                                headDistance = transferredHeadDistance + queueDistance;
                                queue.Dequeue();
                            }
                            else
                            {
                                headDistance = queueDistance;
                                head.distance = headDistance;
                                head.UpdateTransform(position_local, (Directions)i);
                            }
                        }
                        else
                        {
                            head.distance = headDistance;
                            head.UpdateTransform(position_local, (Directions)i);
                        }
                    }
                    else
                    {
                        Assert.IsTrue(queueDistance - minItemDistance >= headDistance);
                        Assert.IsTrue(queueDistance - minItemDistance <= headDistance + fixedItemSpeed);
                        headDistance = Mathf.Min(headDistance, queueDistance - minItemDistance);
                        head.distance = headDistance;
                        head.UpdateTransform(position_local, (Directions)i);
                    }
                }

                float lookAheadItemDistance = headDistance;

                for (int j = queueHead + 1, jiter = 1, jlen = queue.Count, queueArrayLen = queueArray.Length; jiter < jlen; ++j, ++jiter)
                {
                    if (j == queueArrayLen)
                    {
                        j = 0;
                    }
                    ref ConveyorItem item = ref queueArray[j];
                    float itemDistance = item.distance;
                    itemDistance = Mathf.Min(itemDistance + fixedItemSpeed, Mathf.Max(itemDistance, lookAheadItemDistance - minItemDistance));
                    item.distance = itemDistance;
                    item.UpdateTransform(position_local, (Directions)i);
                    lookAheadItemDistance = itemDistance;
                }
            }
        }
    }
}
