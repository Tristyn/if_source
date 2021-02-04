using System;
using UnityEngine;

public struct ConveyorSegment
{
    public OpenQueue<ConveyorItem> queue;
    public ConveyorItem? outputSlot;
    public Conveyor outputConveyor;

    public Save save;
    public struct Save
    {
        public ConveyorItem.Save[] queue;
        public ConveyorItem.Save? outputSlot;
    }

    public void GetSave(out Save save)
    {
        int jlen = queue.Count;
        if (jlen == 0)
        {
            save.queue = Array.Empty<ConveyorItem.Save>();
        }
        else
        {
            ConveyorItem.Save[] saveItems = new ConveyorItem.Save[jlen];
            save.queue = saveItems;
            ConveyorItem[] queueArray = queue.array;
            for (int j = queue.head, jiter = 0, queueArrayLen = queueArray.Length; jiter < jlen; ++j, ++jiter)
            {
                if (j == queueArrayLen)
                {
                    j = 0;
                }
                queueArray[j].GetSave(out saveItems[jiter]);
            }
        }
        if (outputSlot != null)
        {
            outputSlot.Value.GetSave(out ConveyorItem.Save conveyorItemSave);
            save.outputSlot = conveyorItemSave;
        }
        else
        {
            save.outputSlot = default;
        }
    }

    public void SetSave(in Save save)
    {
        for (int j = 0, jLen = save.queue.Length; j < jLen; ++j)
        {
            ref ConveyorItem.Save saveConveyorItem = ref save.queue[j];
            ItemInfo itemInfo = ScriptableObjects.instance.GetItemInfo(saveConveyorItem.itemName);
            if (itemInfo)
            {
                Item item = ItemPooler.instance.Get(itemInfo);
                ConveyorItem conveyorItem = new ConveyorItem(item, saveConveyorItem.distance);
                queue.Enqueue(conveyorItem);
            }
            else
            {
                Debug.LogWarning($"Failed to find item {saveConveyorItem.itemName} while loading conveyor.");
            }
        }
    }

    public bool valid => queue != null;

    public bool hasItems
    {
        get
        {
            return queue.Count > 0;
        }
        set
        {
            ClearItems();
        }
    }

    public bool PlaceItem(ItemInfo itemInfo)
    {
        // it is intentional to not check distance of other queues because items are only placed
        // inside machines and there we only care about the singular machine output.
        // When items transfer between conveyor queues we check distance on every queue.
        if (queue.Count == 0 || queue.array[queue.tail].distance >= Conveyor.minItemDistance)
        {
            Item item = ItemPooler.instance.Get(itemInfo);
            ConveyorItem conveyorItem = new ConveyorItem(item);
            queue.Enqueue(conveyorItem);
            return true;
        }

        return false;
    }

    public void ClearItems()
    {
        while (queue.Count > 0)
        {
            queue.Dequeue().GetItem().EvictedFromConveyor();
        }
    }
}