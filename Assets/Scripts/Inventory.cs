using System;
using UnityEngine.Assertions;

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
            ++count;
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

    public static InventorySlot Defaults(AssembleSlot assembleSlot)
    {
        return new InventorySlot
        {
            itemInfo = assembleSlot.itemInfo,
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
        for (int i = 0, len = slots.Length; i < len; ++i)
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
        for (int i = 0, len = slots.Length; i < len; ++i)
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
        for (int i = 0, len = slots.Length; i < len; ++i)
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
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].TryDecrement())
            {
                return true;
            }
        }
        return false;
    }

    public Inventory Clone()
    {
        int slotsLength = slots.Length;
        InventorySlot[] destinationSlots = new InventorySlot[slotsLength];
        Array.Copy(slots, destinationSlots, slotsLength);
        return new Inventory
        {
            slots = destinationSlots
        };
    }

    public bool HasItem(ItemInfo itemInfo)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (itemInfo == slots[i].itemInfo)
            {
                if (slots[i].count < 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        Assert.IsTrue(false, "Inventory is missing the item slot");
        return false;
    }

    public bool HasItem(AssembleSlot item)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (item.itemInfo == slots[i].itemInfo)
            {
                if (slots[i].count < item.count)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        Assert.IsTrue(false, "Inventory is missing the item slot");
        return false;
    }

    public bool HasItems(AssembleSlot[] items)
    {
        // Why are we doing an O(n*m) algorithm here instead of keep inputs and inventory as dictionarys?
        // Arrays are faster than dictionaries roughly when count < 10, and these collections don't get bigger than 5
        for (int i = 0, len = items.Length; i < len; ++i)
        {
            AssembleSlot item = items[i];
            for (int j = 0, lenj = slots.Length; j < lenj; ++j)
            {
                if (item.itemInfo == slots[j].itemInfo)
                {
                    if (slots[j].count < items[i].count)
                    {
                        return false;
                    }
                    else
                    {
                        goto nextInput;
                    }
                }
            }
            Assert.IsTrue(false, "Inventory is missing the item slot");
            return false;

        nextInput:;
        }

        return true;
    }

    public void DeductItem(ItemInfo itemInfo)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (itemInfo == slots[i].itemInfo)
            {
                slots[i].count -= 1;
                Assert.IsTrue(slots[i].count >= 0);
            }
        }
    }

    public void DeductItem(AssembleSlot item)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (item.itemInfo == slots[i].itemInfo)
            {
                slots[i].count -= item.count;
                Assert.IsTrue(slots[i].count >= 0);
            }
        }
    }

    public void DeductItems(AssembleSlot[] items)
    {
        for (int i = 0, len = items.Length; i < len; ++i)
        {
            AssembleSlot item = items[i];
            for (int j = 0, lenj = slots.Length; j < lenj; ++j)
            {
                if (item.itemInfo == slots[j].itemInfo)
                {
                    slots[j].count -= item.count;
                    Assert.IsTrue(slots[j].count >= 0);
                }
            }
        }
    }
}

