﻿using Assets.Scripts.Consts;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public struct ConveyorItem
{
    public Transform itemTransform;
    public float distance;
    public Vector3 conveyorQueueOrigin_local;
    public Directions direction;

    public ConveyorItem(Item item, Vector3 conveyorQueueOrigin, Directions direction)
    {
        itemTransform = item.transform;
        distance = 0;
        this.conveyorQueueOrigin_local = conveyorQueueOrigin;
        this.direction = direction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Item GetItem()
    {
        return itemTransform.GetComponent<Item>();
    }

    public void UpdateTransform()
    {
        Vector3 offset = direction.ToOffset(distance);
        Vector3 position_local = conveyorQueueOrigin_local + offset;
        itemTransform.localPosition = position_local;
    }
}

public class Conveyor : MonoBehaviour
{
    public Vector3Int position_local;
    public Machine machine;
    public Conveyor[] neighbors = new Conveyor[EnumUtils<Directions>.valuesLength];
    public Conveyor[] inputs = new Conveyor[EnumUtils<Directions>.valuesLength];
    public Conveyor[] outputs = new Conveyor[EnumUtils<Directions>.valuesLength];
    public OpenQueue<ConveyorItem>[] outputQueues = new OpenQueue<ConveyorItem>[EnumUtils<Directions>.valuesLength];
    public ConveyorLink[] outputLinks = new ConveyorLink[EnumUtils<Directions>.valuesLength];

    Directions currentRouterInput = Directions.None;
    Directions lastRoutedDirection = Directions.None;

    public const float minItemDistance = 0.5000001f;
    public const float queueDistance = 1f;
    public const float itemSpeed = 1f;
    public const float fixedItemSpeed = itemSpeed / TimeHelper.fixedFrameRate;

    public static Conveyor CreateConveyor(Vector3Int position)
    {
        if (!ConveyorSystem.instance.CanCreate(position))
        {
            return null;
        }
        if (ConveyorSystem.instance.conveyors.TryGetValue(position, out Conveyor conveyor))
        {
            return conveyor;
        }

        conveyor = DoCreateConveyor(position);
        if (conveyor)
        {
            AudioSystem.instance.PlayOneShot(ConveyorSystem.instance.createConveyorClip, AudioCategory.Effect);
        }
        return conveyor;
    }

    private static Conveyor DoCreateConveyor(Vector3Int position)
    {
        Conveyor conveyor = ObjectPooler.instance.Get<Conveyor>();
        conveyor.transform.localPosition = position.RoundToTileCenter();
        conveyor.Initialize();
        return conveyor;
    }

    public static Conveyor CreateConveyor(Vector3Int from, Vector3Int to)
    {
        if (!ConveyorSystem.instance.CanLink(from, to))
        {
            return null;
        }

        if ((from - to).ToDirection() == Directions.None)
        {
            Debug.LogWarning("Can't link conveyors that aren't neighbors.");
            return null;
        }
        bool playAudio = false;

        if (!ConveyorSystem.instance.conveyors.TryGetValue(to, out Conveyor conveyor))
        {
            conveyor = DoCreateConveyor(to);
            playAudio = playAudio || conveyor;
        }
        if (!ConveyorSystem.instance.conveyors.TryGetValue(from, out Conveyor sourceConveyor))
        {
            sourceConveyor = DoCreateConveyor(from);
            playAudio = playAudio || sourceConveyor;
        }
        if (conveyor && sourceConveyor && !sourceConveyor.IsLinked(conveyor))
        {
            sourceConveyor.Link(conveyor);
            playAudio = true;
        }
        if (playAudio)
        {
            AudioSystem.instance.PlayOneShot(ConveyorSystem.instance.createConveyorClip, AudioCategory.Effect);
        }
        return conveyor;
    }

    public void Initialize()
    {
        position_local = transform.localPosition.RoundToTile();
        ConveyorSystem.instance.Add(this);
        if (MachineSystem.instance.GetMachine(position_local, out Machine machine))
        {
            this.machine = machine;
            machine.FindConveyors();
        }
    }

    public void Recycle()
    {
        Directions[] directions = EnumUtils<Directions>.nonZeroValues;
        currentRouterInput = Directions.None;
        lastRoutedDirection = Directions.None;

        // Remove neighbors
        for (int i = 0, len = directions.Length; i < len; ++i)
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

        ConveyorSystem.instance.Remove(position_local);
        if (machine)
        {
            machine.FindConveyors();
        }
        ObjectPooler.instance.Recycle(this);
    }

    public void PlayDemolishAudio()
    {
        AudioSystem.instance.PlayOneShot(ConveyorSystem.instance.demolishConveyorClip, AudioCategory.Effect);
    }

    public void Link(Conveyor to)
    {
        Directions direction = (to.position_local - position_local).ToDirection();
        if (direction != Directions.None && !outputs[(int)direction])
        {
            to.Unlink(this);

            outputs[(int)direction] = to;
            inputs[(int)direction] = null;
            outputQueues[(int)direction] = new OpenQueue<ConveyorItem>();
            Directions inverseDirection = direction.Inverse();
            to.inputs[(int)inverseDirection] = this;
            to.outputs[(int)inverseDirection] = null;

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
    }

    public void Unlink(Conveyor to)
    {
        Directions direction = (to.position_local - position_local).ToDirection();
        if (direction != Directions.None && outputs[(int)direction])
        {
            Directions inverseDirection = direction.Inverse();
            Assert.IsNull(inputs[(int)direction]);
            Assert.IsNull(to.outputs[(int)inverseDirection]);

            ClearItemQueue(outputQueues[(int)direction]);
            outputQueues[(int)direction] = null;
            outputs[(int)direction] = null;
            to.inputs[(int)inverseDirection] = null;

            outputLinks[(int)direction].Recycle();
            outputLinks[(int)direction] = null;

            if (to.currentRouterInput == direction)
            {
                to.currentRouterInput = Directions.None;
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
    }

    public bool IsLinked()
    {
        for (int i = 1, len = outputs.Length; i < len; ++i)
        {
            if (outputs[i] || inputs[i])
            {
                return true;
            }
        }
        return false;
    }

    public bool IsLinked(Conveyor to)
    {
        Assert.IsNotNull(to);
        if (!to)
        {
            return false;
        }
        for (int i = 1, len = outputs.Length; i < len; ++i)
        {
            if (outputs[i] == to)
            {
                return true;
            }
        }
        return false;
    }

    void ClearItemQueue(OpenQueue<ConveyorItem> itemQueue)
    {
        while (itemQueue.Count > 0)
        {
            itemQueue.Dequeue().GetItem().EvictedFromConveyor();
        }
    }

    public bool PlaceItem(ItemInfo itemInfo, Directions direction)
    {
        Assert.IsNotNull(outputs[(int)direction]);
        OpenQueue<ConveyorItem> items = outputQueues[(int)direction];

        // it is intentional to not check distance of other queues because items are only placed
        // inside machines and there we only care about the singular machine output.
        // When items transfer between conveyor queues we check distance on every queue.
        if (items.Count == 0 || items.array[items.tail].distance >= minItemDistance)
        {
            Item item = ItemPooler.instance.Get(itemInfo);
            ConveyorItem conveyorItem = new ConveyorItem(item, transform.localPosition, direction);
            conveyorItem.UpdateTransform();
            items.Enqueue(conveyorItem);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Find the next route direction in the sequence
    /// </summary>
    public OpenQueue<ConveyorItem> GetRouteDirection(out Directions direction)
    {
        OpenQueue<ConveyorItem>[] queues = outputQueues;
        for (int i = (int)lastRoutedDirection + 1, len = queues.Length; i < len; ++i)
        {
            OpenQueue<ConveyorItem> queue = queues[i];
            if (queue == null)
            {
                continue;
            }
            direction = (Directions)i;
            return queue;
        }
        for (int i = 1, len = (int)lastRoutedDirection + 1; i < len; ++i)
        {
            OpenQueue<ConveyorItem> queue = queues[i];
            if (queue == null)
            {
                continue;
            }
            direction = (Directions)i;
            return queue;
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
        OpenQueue<ConveyorItem>[] queues = outputQueues;
        for (int i = (int)lastRoutedDirection + 1, len = queues.Length; i < len; ++i)
        {
            if (!outputs[i])
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = queues[i];
            Assert.IsNotNull(queue);
            ConveyorItem[] queueArray = queue.array;
            int queueTail = queue.tail;
            if (queue.Count == 0)
            {
                routedItemDistance = queueDistance;
                direction = (Directions)i;
                destination = queue;
                return true;
            }
            else if (queueArray[queueTail].distance - minItemDistance < 0)
            {
                continue;
            }

            routedItemDistance = queueArray[queueTail].distance;
            direction = (Directions)i;
            destination = queue;
            return true;
        }
        for (int i = 1, len = (int)lastRoutedDirection + 1; i < len; ++i)
        {
            if (!outputs[i])
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = queues[i];
            Assert.IsNotNull(queue);
            ConveyorItem[] queueArray = queue.array;
            int queueTail = queue.tail;
            if (queue.Count == 0)
            {
                routedItemDistance = queueDistance;
                direction = (Directions)i;
                destination = queue;
                return true;
            }
            else if (queueArray[queueTail].distance - minItemDistance < 0)
            {
                continue;
            }

            routedItemDistance = queueArray[queueTail].distance;
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
        if (machine)
        {
            return distance;
        }
        OpenQueue<ConveyorItem>[] queues = outputQueues;
        for (int i = 1, len = queues.Length; i < len; ++i)
        {
            OpenQueue<ConveyorItem> queue = queues[i];
            if (queue == null || queue.Count == 0)
            {
                continue;
            }
            distance = Mathf.Min(distance, queue.array[queue.tail].distance);
        }
        return distance;
    }

    public bool BeginTransferIn(Directions outputDirection)
    {
        if (currentRouterInput == outputDirection)
        {
            return true;
        }
        if (currentRouterInput == Directions.None)
        {
            currentRouterInput = outputDirection;
            return true;
        }
        return false;
    }

    public bool TransferIn(ConveyorItem conveyorItem)
    {
        if (machine)
        {
            if (machine.inventory.TryIncrement(conveyorItem.GetItem().itemInfo))
            {
                conveyorItem.GetItem().ConsumedByMachine();
                return true;
            }
            return false;
        }
        else
        {
            if (GetRouteDirection(out Directions direction, out OpenQueue<ConveyorItem> destination, out float routedItemDistance) && routedItemDistance >= minItemDistance)
            {
                lastRoutedDirection = direction;
                currentRouterInput = Directions.None;
                conveyorItem.direction = direction;
                conveyorItem.distance = Mathf.Max(0f, Mathf.Min(fixedItemSpeed, routedItemDistance - minItemDistance));
                conveyorItem.conveyorQueueOrigin_local = transform.localPosition;
                conveyorItem.UpdateTransform();
                destination.Enqueue(conveyorItem);
                return true;
            }
            return false;
        }
    }

    void FixedUpdate()
    {
        /* Updating a conveyor queue updates 4 types of items
         * - The outgoing item transitioning to the machine, the router or another conveyor
         * - Other outgoing items
         * - The incoming item entering the machine or the router, the head item
         * - Other incoming items
         * each item relies on the lookAheadItemDistance from the previous item so 
         * it's all done in one monolithic update.
         */

        for (int i = 1, len = outputs.Length; i < len; ++i)
        {
            Conveyor outputConveyor = outputs[i];
            if (outputConveyor)
            {
                OpenQueue<ConveyorItem> queue = outputQueues[i];
                Assert.IsNotNull(queue);
                int queueHead = queue.head;
                ConveyorItem[] queueArray = queue.array;
                if (queue.Count == 0)
                {
                    continue;
                }
                float headDistance = queueArray[queueHead].distance;
                if (headDistance + fixedItemSpeed + minItemDistance < queueDistance)
                {
                    headDistance = headDistance + fixedItemSpeed;
                    queueArray[queueHead].distance = headDistance;
                    queueArray[queueHead].UpdateTransform();
                }
                else
                {
                    if (outputConveyor.BeginTransferIn((Directions)i))
                    {
                        headDistance = Mathf.Min(headDistance + fixedItemSpeed, queueDistance);
                        if (headDistance == queueDistance)
                        {
                            // We only transfer when all queues have enough space
                            // That's why it doesn't need to check for space on each route once it's transferred and routed
                            ConveyorItem head = queueArray[queueHead];
                            if (outputConveyor.TransferIn(head))
                            {
                                queue.Dequeue();
                            }
                            else
                            {
                                queueArray[queueHead].distance = headDistance;
                                queueArray[queueHead].UpdateTransform();
                            }
                        }
                        else
                        {
                            queueArray[queueHead].distance = headDistance;
                            queueArray[queueHead].UpdateTransform();
                        }
                    }
                    else
                    {
                        Assert.IsTrue(queueDistance - minItemDistance >= headDistance);
                        Assert.IsTrue(queueDistance - minItemDistance <= headDistance + fixedItemSpeed);
                        headDistance = Mathf.Min(headDistance, queueDistance - minItemDistance);
                        queueArray[queueHead].distance = headDistance;
                        queueArray[queueHead].UpdateTransform();
                    }
                }

                float lookAheadItemDistance = headDistance;

                for (int j = 1, lenJ = queue.Count; j < lenJ; ++j)
                {
                    int itemIndex = queue.GetElementIndex(j);
                    float itemDistance = queueArray[itemIndex].distance;
                    itemDistance = Mathf.Min(itemDistance + fixedItemSpeed, Mathf.Max(itemDistance, lookAheadItemDistance - minItemDistance));
                    queueArray[itemIndex].distance = itemDistance;
                    queueArray[itemIndex].UpdateTransform();
                    lookAheadItemDistance = itemDistance;
                }
            }
        }
    }
}
