using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachinePurchaser : MonoBehaviour
{
    public Machine machine;

    Inventory inventory;
    AssembleSlot purchaseItem;
    float placeInterval;

    [Serializable]
    public struct Save
    {
        public float nextPlaceTime;
    }

    [NonSerialized]
    public Save save;

    public void Initialize()
    {
        save.nextPlaceTime = GameTime.fixedTime;
        inventory = machine.inventory;
        purchaseItem = machine.machineInfo.purchaseItem;
        placeInterval = machine.machineInfo.placeInterval;
    }

    void FixedUpdate()
    {
        if (save.nextPlaceTime <= GameTime.fixedTime)
        {
            save.nextPlaceTime += placeInterval;

            CurrencySystem currencySystem = CurrencySystem.instance;
            if (currencySystem.CanPurchaseItem(purchaseItem.itemInfo, purchaseItem.count))
            {
                ref InventorySlot slot = ref inventory.GetSlot(purchaseItem.itemInfo);
                Assert.IsTrue(slot.valid);
                if (slot.TryAdd(purchaseItem.count))
                {
                    currencySystem.MachinePurchaserBuyItem(purchaseItem.itemInfo, purchaseItem.count);
                }
            }
        }
    }
}