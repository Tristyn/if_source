﻿using GameAnalyticsSDK;
using System;
using UnityEngine;
using UnityEngine.Events;

public enum CurrencyType : byte
{
    Money,
    Xp,
    Level,
}

public sealed class CurrencySystem : Singleton<CurrencySystem>
{
    [Serializable]
    public struct Save
    {
        public long money;
        public long xp;
        public long level;

        public bool itemsCostMoney;

        public long this[CurrencyType type]
        {
            get
            {
                switch (type)
                {
                    case CurrencyType.Money:
                        return money;
                    case CurrencyType.Xp:
                        return xp;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (type)
                {
                    case CurrencyType.Money:
                        money = value;
                        break;
                    case CurrencyType.Xp:
                        xp = value;
                        break;
                }
            }
        }
    }

    [NonSerialized]
    public Save save;

    public long conveyorCost = 10;

    public Vector3 currencySpawnOffset;

    public UnityEvent moneyChanged;
    public UnityEvent xpChanged;
    public UnityEvent levelChanged;
    public UnityEvent freeItemsChanged;

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.LoadComplete += LoadComplete;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveLoad.LoadComplete -= LoadComplete;
    }

    void LoadComplete()
    {
        moneyChanged.Invoke();
        xpChanged.Invoke();
    }

    public void BuildMachine(MachineInfo machineInfo)
    {
        if (save.itemsCostMoney)
        {
            long value = machineInfo.cost;
            save.money -= value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Sink, CurrencyType.Money, CurrencyEventType.MachinePurchased, machineInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public void SellMachine(MachineInfo machineInfo)
    {
        if (save.itemsCostMoney)
        {
            long value = machineInfo.cost;
            save.money += value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.MachineSold, machineInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public bool CanBuildMachine(MachineInfo machineInfo)
    {
        if (!save.itemsCostMoney)
        {
            return true;
        }

        return machineInfo.cost <= 0 || save.money >= machineInfo.cost;
    }

    public void BuyConveyors(int count)
    {
        if (save.itemsCostMoney)
        {
            long value = conveyorCost * count;
            save.money -= value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Sink, CurrencyType.Money, CurrencyEventType.ConveyorPurchased, "Conveyor");
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public void SellConveyor()
    {
        if (save.itemsCostMoney)
        {
            long value = conveyorCost;
            save.money += value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ConveyorSold, "Conveyor");
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public bool CanBuyConveyor(int count)
    {
        if (!save.itemsCostMoney)
        {
            return true;
        }

        long value = conveyorCost * count;
        return value <= 0 || save.money >= value;
    }

    public void Buy(long count, long value, CurrencyType currencyType)
    {
        if (save.itemsCostMoney)
        {
            value = value * count;
            save[currencyType] -= value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Sink, currencyType, CurrencyEventType.ItemPurchased, "Item");
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public void Sell(long count, long value, CurrencyType currencyType)
    {
        if (save.itemsCostMoney)
        {
            value = value * count;
            save[currencyType] += value;

            switch (currencyType)
            {
                case CurrencyType.Money:
                    moneyChanged.Invoke();
                    break;
                case CurrencyType.Xp:
                    xpChanged.Invoke();
                    break;
                case CurrencyType.Level:
                    levelChanged.Invoke();
                    break;
            }

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, currencyType, CurrencyEventType.ItemSold, "Item");
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public bool CanBuy(long count, long value, CurrencyType currencyType)
    {
        if (!save.itemsCostMoney)
        {
            return true;
        }

        value = value * count;
        return value <= 0 || save[currencyType] >= value;
    }

    public void MachineSellerSellItem(ItemInfo itemInfo, int count, Vector3 position)
    {
        if (save.itemsCostMoney)
        {
            long value = itemInfo.value * count;
            save.money += value;

            moneyChanged.Invoke();

            CurrencyMoney currency = ObjectPooler.instance.Get<CurrencyMoney>();
            currency.Initialize(position);

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.MachineSellerRevenue, itemInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public void MachinePurchaserBuyItem(ItemInfo itemInfo, int count)
    {
        if (save.itemsCostMoney)
        {
            long value = itemInfo.value * count;
            save.money -= value;

            moneyChanged.Invoke();

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Sink, CurrencyType.Money, CurrencyEventType.MachinePurchaserFees, itemInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);
        }
    }

    public void RefundInventory(Inventory inventory)
    {
        if (save.itemsCostMoney)
        {
            long valueSum = 0;
            for (int i = 0, len = inventory.slots.Length; i < len; ++i)
            {
                InventorySlot slot = inventory.slots[i];
                if (slot.count > 0)
                {
                    long value = slot.itemInfo.value * slot.count;
                    valueSum += value;

                    CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ItemRefunded, slot.itemInfo);
                    Analytics.instance.NewCurrencyEvent(currencyEvent, value);
                }
            }

            save.money += valueSum;
            moneyChanged.Invoke();
        }
    }

    public void RefundItem(ItemInfo itemInfo, int count)
    {
        if (save.itemsCostMoney)
        {
            long value = itemInfo.value * count;
            save.money += value;

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ItemRefunded, itemInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);

            moneyChanged.Invoke();
        }
    }

    public void ProgressionReward(ProgressionInfo progressionInfo)
    {
        if (progressionInfo.moneyReward > 0)
        {
            long value = progressionInfo.moneyReward;
            save.money += value;

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ProgressionReward, progressionInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);

            moneyChanged.Invoke();
        }

        if (progressionInfo.xpReward > 0)
        {
            long value = progressionInfo.xpReward;
            save.xp += value;

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Xp, CurrencyEventType.ProgressionReward, progressionInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);

            xpChanged.Invoke();
        }
    }

    public bool CanPurchaseItem(ItemInfo itemInfo, int count)
    {
        if (!save.itemsCostMoney)
        {
            return true;
        }

        long value = itemInfo.value * count;
        return value <= 0 || save.money >= value;
    }

    public void SetMoney(long value)
    {
        save.money = value;
        moneyChanged.Invoke();
    }

    public void SetItemsCostMoney(bool itemsCostMoney)
    {
        save.itemsCostMoney = itemsCostMoney;
        freeItemsChanged.Invoke();
    }
}