using System;

[Serializable]
public struct InventorySlot
{
    public ItemInfo itemInfo;
    public int count;
    public int capacity;

    public bool TryAdd(int count)
    {
        if (this.count + count <= capacity)
        {
            this.count += count;
            return true;
        }
        return false;
    }

    public bool TryRemove(int count)
    {
        if (this.count - count >= 0)
        {
            this.count -= count;
            return true;
        }
        return false;
    }

    public bool TryIncrement()
    {
        if (count < capacity)
        {
            count++;
            return true;
        }
        return false;
    }

    public bool TryDecrement()
    {
        if (count > 0)
        {
            count--;
            return true;
        }
        return false;
    }

    public static InventorySlot Defaults(ItemInfo itemInfo)
    {
        return new InventorySlot
        {
            itemInfo = itemInfo,
            count = 0,
            capacity = 10
        };
    }
}

/// <summary>
/// An inventory where each slot is reserved by type
/// </summary>
[Serializable]
public struct Inventory
{
    public InventorySlot[] slots;

    public static Inventory empty = new Inventory
    {
        slots = Array.Empty<InventorySlot>()
    };

    public bool TryAdd(ItemInfo itemInfo, int count)
    {
        for (int i = 0, len = slots.Length; i < len; i++)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].TryAdd(count))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryRemove(ItemInfo itemInfo, int count)
    {
        for (int i = 0, len = slots.Length; i < len; i++)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].TryRemove(count))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryIncrement(ItemInfo itemInfo)
    {
        for (int i = 0, len = slots.Length; i < len; i++)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].TryIncrement())
            {
                return true;
            }
        }
        return false;
    }

    public bool TryDecrement(ItemInfo itemInfo)
    {
        for (int i = 0, len = slots.Length; i < len; i++)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].TryDecrement())
            {
                return true;
            }
        }
        return false;
    }

    public void Clone(out Inventory destination)
    {
        int slotsLength = slots.Length;
        InventorySlot[] destinationSlots = new InventorySlot[slotsLength];
        Array.Copy(slots, destinationSlots, slotsLength);
        destination = new Inventory
        {
            slots = destinationSlots
        };
    }
}

