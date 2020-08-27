using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public enum InventorySlotType : byte
{
    None = 0,
    Input = 1,
    Output = 2,
    InputOutput = 3
}

[Serializable]
public struct InventorySlot
{
    public ItemInfo itemInfo;
    public int count;
    public int capacity;
    public InventorySlotType inventorySlotType;

    public static InventorySlot Invalid;

    public bool valid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return itemInfo; }
    }

    public bool empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return count <= 0; }
    }

    [Serializable]
    public struct Save
    {
        public string itemName;
        public int count;
    }

    public void GetSave(out Save save)
    {
        save.itemName = itemInfo.name;
        save.count = count;
    }

    public void SetSave(in Save save)
    {
        Assert.IsTrue(itemInfo.itemName == save.itemName);
        count = save.count;
    }

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

    public static InventorySlot Create(AssembleSlot assembleSlot, InventorySlotType inventorySlotType)
    {
        return new InventorySlot
        {
            itemInfo = assembleSlot.itemInfo,
            count = 0,
            capacity = assembleSlot.count * 2,
            inventorySlotType = inventorySlotType
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

    public static Inventory empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Inventory
        {
            slots = Array.Empty<InventorySlot>()
        };
    }

    public static Inventory invalid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    public bool valid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => slots != null;
    }

    [Serializable]
    public struct Save
    {
        public InventorySlot.Save[] slots;
    }

    public static Inventory CreateInventory(MachineInfo machineInfo)
    {
        int numSlots = 0;
        if (machineInfo.purchaseItem.itemInfo)
        {
            ++numSlots;
        }
        if (machineInfo.sellItem.itemInfo)
        {
            ++numSlots;
        }
        if (machineInfo.assembler)
        {
            numSlots += machineInfo.assembleInputs.Length;
            if (machineInfo.assembleOutput.itemInfo)
            {
                ++numSlots;
            }
        }
        InventorySlot[] slots = new InventorySlot[numSlots];
        int inventorySlotsIndex = 0;

        // inventory order is purchase item, sell item, assemble inputs, assemble outputs, anything else
        if (machineInfo.purchaseItem.itemInfo)
        {
            slots[inventorySlotsIndex] = InventorySlot.Create(machineInfo.purchaseItem, InventorySlotType.Output);
            ++inventorySlotsIndex;
        }
        if (machineInfo.sellItem.itemInfo)
        {
            slots[inventorySlotsIndex] = InventorySlot.Create(machineInfo.sellItem, InventorySlotType.Input);
            ++inventorySlotsIndex;
        }

        if (machineInfo.assembler)
        {
            AssembleSlot[] assembleInputs = machineInfo.assembleInputs;
            for (int i = 0, len = assembleInputs.Length; i < len; ++i)
            {
                slots[inventorySlotsIndex] = InventorySlot.Create(machineInfo.assembleInputs[i], InventorySlotType.Input);
                ++inventorySlotsIndex;
            }

            slots[inventorySlotsIndex] = InventorySlot.Create(machineInfo.assembleOutput, InventorySlotType.Output);
            ++inventorySlotsIndex;
        }
        Assert.IsTrue(inventorySlotsIndex == slots.Length);

        return new Inventory
        {
            slots = slots
        };
    }

    public void GetSave(out Save save)
    {
        InventorySlot[] slots = this.slots;
        int len = slots.Length;
        InventorySlot.Save[] saveSlots = new InventorySlot.Save[len];
        for (int i = 0; i < len; ++i)
        {
            slots[i].GetSave(out saveSlots[i]);
        }

        save.slots = saveSlots;
    }

    public void SetSave(in Save save)
    {
        // The inventory should have been initialized with itemInfo and capacity values at this point
        InventorySlot.Save[] saveSlots = save.slots;
        int len = saveSlots.Length;
        for (int i = 0; i < len; ++i)
        {
            ItemInfo itemInfo = ScriptableObjects.instance.GetItemInfo(saveSlots[i].itemName);
            InventorySlot slot;
            if (itemInfo)
            {
                if ((slot = GetSlot(itemInfo)).valid)
                {
                    slot.SetSave(in saveSlots[i]);
                }
                else
                {
                    Debug.LogWarning($"Failed to find item slot {saveSlots[i].itemName} while loading inventory.");
                }
            }
        }
    }

    // Must check if slot is valid before accessing
    public ref InventorySlot GetSlot(ItemInfo itemInfo)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (slots[i].itemInfo == itemInfo)
            {
                return ref slots[i];
            }
        }
        return ref InventorySlot.Invalid;
    }

    // Must check if slot is valid before accessing
    public ref InventorySlot GetSlot(ItemInfo itemInfo, InventorySlotType inventorySlotType)
    {
        for (int i = 0, len = slots.Length; i < len; ++i)
        {
            if (slots[i].itemInfo == itemInfo && slots[i].inventorySlotType == inventorySlotType)
            {
                return ref slots[i];
            }
        }
        return ref InventorySlot.Invalid;
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

