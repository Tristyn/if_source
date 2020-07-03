using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public struct ConveyorItem
{
    public Transform itemTransform;
    public float distance;
    public Vector3 conveyorQueueOrigin;
    public Directions direction;

    public ConveyorItem(Item item, Vector3 conveyorQueueOrigin, Directions direction)
    {
        itemTransform = item.transform;
        distance = 0;
        this.conveyorQueueOrigin = conveyorQueueOrigin;
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
        Vector3 position = conveyorQueueOrigin + offset;
        itemTransform.position = position;
    }
}

public class Conveyor : MonoBehaviour
{
    public Vector3Int position;
    [NonSerialized]
    public Conveyor[] neighbors = new Conveyor[EnumUtils<Directions>.valuesLength];
    [NonSerialized]
    public Conveyor[] inputs = new Conveyor[EnumUtils<Directions>.valuesLength];
    [NonSerialized]
    public Conveyor[] outputs = new Conveyor[EnumUtils<Directions>.valuesLength];
    [NonSerialized]
    public ConveyorLink[] outputLinks = new ConveyorLink[EnumUtils<Directions>.valuesLength];
    [NonSerialized]
    public OpenQueue<ConveyorItem>[] itemQueues = new OpenQueue<ConveyorItem>[EnumUtils<Directions>.valuesLength];
    [NonSerialized]
    public Machine machine;

    Directions currentRouterInput = Directions.None;
    Directions lastRoutedDirection = Directions.None;

    const float minItemDistance = 0.2f;
    const float queueDistance = 0.5f;
    const float itemSpeed = 1f / 30f;

    public static Conveyor CreateConveyor(Vector3Int position)
    {
        if (ConveyorSystem.instance.conveyors.TryGetValue(position, out Conveyor conveyor))
        {
            return conveyor;
        }

        conveyor = ObjectPooler.instance.Get<Conveyor>();
        conveyor.transform.position = position.RoundToTileCenter();
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
            this.machine = machine;
            machine.FindConveyors();
        }
    }

    public void Recycle()
    {
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
        if (machine)
        {
            machine.FindConveyors();
        }
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
            itemQueues[(int)direction] = new OpenQueue<ConveyorItem>();
            Directions inverseDirection = direction.Inverse();
            to.inputs[(int)inverseDirection] = this;
            to.outputs[(int)inverseDirection] = null;
            to.itemQueues[(int)inverseDirection] = new OpenQueue<ConveyorItem>();

            ConveyorLink link = ObjectPooler.instance.Get<ConveyorLink>();
            outputLinks[(int)direction] = link;
            link.transform.SetParent(transform);
            link.transform.position = transform.position;
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
        Directions direction = (to.position - position).ToDirection();
        if (direction != Directions.None && outputs[(int)direction])
        {
            Directions inverseDirection = direction.Inverse();
            Assert.IsNull(inputs[(int)direction]);
            Assert.IsNull(to.outputs[(int)inverseDirection]);

            outputs[(int)direction] = null;
            ClearItemQueue(itemQueues[(int)direction]);
            itemQueues[(int)direction] = null;

            to.inputs[(int)inverseDirection] = null;
            ClearItemQueue(to.itemQueues[(int)inverseDirection]);
            to.itemQueues[(int)inverseDirection] = null;

            outputLinks[(int)direction].Recycle();
            outputLinks[(int)direction] = null;

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
        OpenQueue<ConveyorItem> items = itemQueues[(int)direction];

        // it is intentional to not check distance of other queues because items are only placed
        // inside machines and there we only care about the singular machine output.
        // When items transfer between conveyor queues we check distance on every queue.
        if (items.Count == 0 || items.array[items.tail].distance >= minItemDistance)
        {
            Item item = ItemPooler.instance.Get(itemInfo);
            ConveyorItem conveyorItem = new ConveyorItem(item, transform.position, direction);
            conveyorItem.UpdateTransform();
            items.Enqueue(conveyorItem);
            return true;
        }

        return false;
    }

    bool GetRoute(out Directions direction, out OpenQueue<ConveyorItem> destination, out float routedItemDistance)
    {
        direction = default;
        destination = null;
        bool routeFound = false;
        routedItemDistance = queueDistance;

        // These 2 loops checks every output queue to find the destination queue,
        // and also make sure there is some space on every queue
        OpenQueue<ConveyorItem>[] queues = itemQueues;
        for (int i = (int)lastRoutedDirection + 1, len = queues.Length; i < len; i++)
        {
            if (!outputs[i])
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = queues[i];
            Assert.IsNotNull(queue);
            ConveyorItem[] queueArray = queue.array;
            int queueTail = queue.tail;
            if (queue.Count == 0 || queueArray[queueTail].distance - minItemDistance >= 0)
            {
                return false;
            }
            routedItemDistance = Mathf.Min(routedItemDistance, queueArray[queueTail].distance);
            if (!routeFound)
            {
                routeFound = true;
                direction = (Directions)i;
                destination = queue;
            }
        }
        for (int i = 1, len = (int)lastRoutedDirection + 1; i < len; i++)
        {
            if (!outputs[i])
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = queues[i];
            Assert.IsNotNull(queue);
            ConveyorItem[] queueArray = queue.array;
            int queueTail = queue.tail;
            if (queue.Count == 0 || queueArray[queueTail].distance - minItemDistance >= 0)
            {
                return false;
            }
            routedItemDistance = Mathf.Min(routedItemDistance, queueArray[queueTail].distance);
            if (!routeFound)
            {
                routeFound = true;
                direction = (Directions)i;
                destination = queue;
            }
        }

        return routeFound;
    }

    /// <summary>
    /// Finds the closest item that has passed the router and returns its distance in it's current queue.
    /// </summary>
    float RoutedItemDistance()
    {
        float distance = queueDistance;
        OpenQueue<ConveyorItem>[] queues = itemQueues;
        for (int i = 1, len = queues.Length; i < len; i++)
        {
            if (!outputs[i])
            {
                continue;
            }
            OpenQueue<ConveyorItem> queue = queues[i];
            Assert.IsNotNull(queue);
            if (queue.Count == 0)
            {
                continue;
            }
            distance = Mathf.Min(distance, queue.PeekTail().distance);
        }
        return distance;
    }

    void FixedUpdate()
    {
        /* Updating a conveyor queue updates 4 types of items
         * - The outgoing item transitioning to another conveyor
         * - Other outgoing items
         * - The incoming item entering the machine or the router, the head item
         * - Other incoming items
         * each item relies on the lookAheadItemDistance from the previous item so 
         * it's all done in one monolithic update.
         */

        for (int i = 1, len = outputs.Length; i < len; i++)
        {
            if (outputs[i])
            {
                OpenQueue<ConveyorItem> queue = itemQueues[i];
                Assert.IsNotNull(queue);
                int queueHead = queue.head;
                ConveyorItem[] queueArray = queue.array;
                if (queue.Count == 0)
                {
                    continue;
                }
                float headDistance = queueArray[queueHead].distance;
                if (headDistance + itemSpeed + minItemDistance < queueDistance)
                {
                    headDistance = headDistance + itemSpeed;
                    queueArray[queueHead].distance = headDistance;
                    queueArray[queueHead].UpdateTransform();
                }
                else
                {
                    Directions transferDirection = (Directions)i;
                    Directions inverseDirection = transferDirection.Inverse();
                    Conveyor transferConveyor = outputs[(int)transferDirection];
                    OpenQueue<ConveyorItem> transferQueue = transferConveyor.itemQueues[(int)inverseDirection];
                    float transferItemDistance;
                    if (transferQueue.Count == 0)
                    {
                        transferItemDistance = queueDistance;
                    }
                    else
                    {
                        transferItemDistance = transferQueue.array[transferQueue.tail].distance;
                    }

                    if (headDistance + itemSpeed < queueDistance)
                    {
                        headDistance = Mathf.Min(headDistance + itemSpeed, Mathf.Max(headDistance, transferItemDistance + queueDistance - minItemDistance));
                        queueArray[queueHead].distance = headDistance;
                        queueArray[queueHead].UpdateTransform();
                    }
                    else
                    {
                        float headDistanceTransferred = Mathf.Min(headDistance - queueDistance + itemSpeed, Mathf.Max(headDistance - queueDistance, transferItemDistance - minItemDistance));
                        if (headDistanceTransferred >= 0f)
                        {
                            headDistance = headDistanceTransferred - queueDistance;
                            ConveyorItem head = queue.Dequeue();
                            Assert.IsTrue(headDistanceTransferred >= 0f);
                            head.distance = headDistanceTransferred;
                            head.conveyorQueueOrigin = transferConveyor.transform.position;
                            head.UpdateTransform();
                            transferQueue.Enqueue(head);
                        }
                        else
                        {
                            headDistance = headDistanceTransferred - queueDistance;
                            queueArray[queueHead].distance = headDistance;
                            queueArray[queueHead].UpdateTransform();
                        }
                    }
                }

                float lookAheadItemDistance = headDistance;

                for (int j = 1, lenJ = queue.Count; j < lenJ; j++)
                {
                    int itemIndex = queue.GetElementIndex(j);
                    float itemDistance = queueArray[itemIndex].distance;
                    itemDistance = Mathf.Min(itemDistance + itemSpeed, Mathf.Max(itemDistance, lookAheadItemDistance - minItemDistance));
                    queueArray[itemIndex].distance = itemDistance;
                    queueArray[itemIndex].UpdateTransform();
                    lookAheadItemDistance = itemDistance;
                }
            }
        }

        for (int i = 1, len = inputs.Length; i < len; i++)
        {
            if (inputs[i])
            {
                OpenQueue<ConveyorItem> queue = itemQueues[i];
                Assert.IsNotNull(queue);
                int queueHead = queue.head;
                ConveyorItem[] queueArray = queue.array;
                if (queue.Count == 0)
                {
                    continue;
                }
                float headDistance = queueArray[queueHead].distance;
                if (headDistance + itemSpeed + minItemDistance < queueDistance)
                {
                    headDistance = headDistance + itemSpeed;
                    queueArray[queueHead].distance = headDistance;
                    queueArray[queueHead].UpdateTransform();
                }
                else
                {
                    if (currentRouterInput == Directions.None)
                    {
                        currentRouterInput = (Directions)i;
                    }
                    else if (currentRouterInput != (Directions)i)
                    {
                        Assert.IsTrue(queueDistance - minItemDistance >= headDistance);
                        Assert.IsTrue(queueDistance - minItemDistance <= headDistance + itemSpeed);
                        headDistance = Mathf.Min(headDistance, Mathf.Max(headDistance, queueDistance - minItemDistance));
                        queueArray[queueHead].distance = headDistance;
                        queueArray[queueHead].UpdateTransform();
                    }

                    if (currentRouterInput == (Directions)i)
                    {
                        if (headDistance + itemSpeed < queueDistance)
                        {
                            float routerItemDistance = RoutedItemDistance();
                            headDistance = Mathf.Min(headDistance + itemSpeed, Mathf.Max(headDistance, routerItemDistance + queueDistance - minItemDistance));
                            queueArray[queueHead].distance = headDistance;
                            queueArray[queueHead].UpdateTransform();
                        }
                        else
                        {
                            if (machine)
                            {
                                if (machine.inventory.TryIncrement(queueArray[queueHead].GetItem().itemInfo))
                                {
                                    queue.Dequeue().GetItem().ConsumedByMachine();
                                }
                            }
                            else
                            {
                                bool routeAvailable = GetRoute(out Directions direction, out OpenQueue<ConveyorItem> destination, out float routedItemDistance);
                                float headDistanceTransferred = Mathf.Min(headDistance - queueDistance + itemSpeed, Mathf.Max(headDistance - queueDistance, routedItemDistance - minItemDistance));
                                if (routeAvailable && headDistanceTransferred >= 0f)
                                {
                                    lastRoutedDirection = direction;
                                    currentRouterInput = Directions.None;
                                    ConveyorItem head = queue.Dequeue();
                                    head.direction = direction;
                                    head.distance = headDistanceTransferred;
                                    head.UpdateTransform();
                                    destination.Enqueue(head);
                                }
                                else
                                {
                                    headDistance = headDistanceTransferred + queueDistance;
                                    Assert.IsTrue(headDistance < queueDistance);
                                    Assert.IsTrue(headDistance + itemSpeed >= queueDistance);
                                    queueArray[queueHead].distance = headDistance;
                                    queueArray[queueHead].UpdateTransform();
                                }
                            }
                        }
                    }
                }

                float lookAheadItemDistance = headDistance;

                for (int j = 1, lenJ = queue.Count; j < lenJ; j++)
                {
                    int itemIndex = queue.GetElementIndex(j);
                    float itemDistance = queueArray[itemIndex].distance;
                    itemDistance = Mathf.Min(itemDistance + itemSpeed, Mathf.Max(itemDistance, lookAheadItemDistance - minItemDistance));
                    queueArray[itemIndex].distance = itemDistance;
                    lookAheadItemDistance = itemDistance;
                }
            }
        }
    }
}
