using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineSeller : MonoBehaviour, IFixedUpdate
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
        Updater.machineSellers.Add(this);
    }

    public void Delete()
    {
        Updater.machineSellers.Remove(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        if (save.nextAssembleTime <= GameTime.fixedTime)
        {
            save.nextAssembleTime += placeInterval;

            bool operated = false;

            ref InventorySlot slot = ref inventory.GetSlot(sellItem.itemInfo);
            Assert.IsTrue(slot.valid);
            if (slot.TryRemove(sellItem.count))
            {
                operated = true;
                CurrencySystem.instance.MachineSellerSellItem(sellItem.itemInfo, sellItem.count, machine.bounds.topCenter);
                MachineGroupAchievements.instance.OnMachineItemSold(machineGroup);
            }

            machine.machineEfficiency.Tick(operated);
        }
    }
}
