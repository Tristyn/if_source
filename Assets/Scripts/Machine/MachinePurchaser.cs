using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachinePurchaser : MonoBehaviour, IFixedUpdate
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
        Updates.machinePurchasers.Add(this);
    }

    public void Delete()
    {
        Updates.machinePurchasers.Remove(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        if (save.nextPlaceTime <= GameTime.fixedTime)
        {
            save.nextPlaceTime += placeInterval;

            bool operated = false;

            CurrencySystem currencySystem = CurrencySystem.instance;
            if (currencySystem.CanPurchaseItem(purchaseItem.itemInfo, purchaseItem.count))
            {
                ref InventorySlot slot = ref inventory.GetSlot(purchaseItem.itemInfo);
                Assert.IsTrue(slot.valid);
                if (slot.TryAdd(purchaseItem.count))
                {
                    operated = true;
                    currencySystem.MachinePurchaserBuyItem(purchaseItem.itemInfo, purchaseItem.count);
                }
            }

            machine.machineEfficiency.Tick(operated);
        }
    }
}