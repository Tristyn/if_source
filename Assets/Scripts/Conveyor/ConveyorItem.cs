using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct ConveyorItem
{
    public Transform itemTransform;
    public float distance;

    public struct Save
    {
        public string itemName;
        public float distance;
    }

    public ConveyorItem(Item item)
    {
        itemTransform = item.transform;
        distance = 0;
    }

    public ConveyorItem(Item item, float distance)
    {
        itemTransform = item.transform;
        this.distance = distance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Item GetItem()
    {
        return itemTransform.GetComponent<Item>();
    }

    public void GetSave(out Save save)
    {
        save.itemName = GetItem().itemInfo.itemName;
        save.distance = distance;
    }

    public void UpdateTransform(Vector3 conveyorQueueOrigin_local, Directions direction)
    {
        Vector3 offset = direction.ToOffset(distance);
        Vector3 position_local = conveyorQueueOrigin_local + offset;
        itemTransform.localPosition = position_local;
    }
}
