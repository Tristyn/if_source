using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineSeller : MonoBehaviour
{
    public Machine machine;

    AssembleSlot sellItem;
    Inventory inventory;
    MachineGroupInfo machineGroup;
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
        save.nextAssembleTime = GameTime.fixedTime;
        MachineInfo machineInfo = machine.machineInfo;
        sellItem = machineInfo.sellItem;
        inventory = machine.inventory;
        machineGroup = machineInfo.machineGroup;
        placeInterval = machineInfo.placeInterval;
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
                CurrencySystem.instance.MachineSellerSellItem(sellItem.itemInfo, sellItem.count, machine.bounds.topCenter);
                MachineGroupAchievements.instance.OnMachineItemSold(machineGroup);
            }
        }
    }
}
