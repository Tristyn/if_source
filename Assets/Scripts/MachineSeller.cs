using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineSeller : MonoBehaviour
{
    public Machine machine;

    AssembleSlot sellItem;
    Inventory inventory;
    float placeInterval;

    [NonSerialized]
    public Save save;

    [Serializable]
    public struct Save
    {
        public float nextAssembleTime;
    }

    public void Initialize()
    {
        placeInterval = machine.machineInfo.placeInterval;
        sellItem = machine.machineInfo.sellItem;
        inventory = machine.inventory;
    }

    private void FixedUpdate()
    {
        if (save.nextAssembleTime <= GameTime.fixedTime)
        {
            save.nextAssembleTime += placeInterval;

            ref InventorySlot slot = ref inventory.GetSlot(sellItem.itemInfo);
            Assert.IsTrue(slot.valid);
            if (slot.TryRemove(sellItem.count))
            {
                CurrencySystem.instance.ItemSold(sellItem.itemInfo, sellItem.count, machine.bounds.topCenter);
            }
        }
    }
}
