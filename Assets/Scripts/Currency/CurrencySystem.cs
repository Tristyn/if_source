using GameAnalyticsSDK;
using System;
using UnityEngine;
using UnityEngine.Events;

public enum CurrencyType : byte
{
    Money,
    Xp
}

public sealed class CurrencySystem : Singleton<CurrencySystem>
{
    [Serializable]
    public struct Save
    {
        public long money;
        public long xp;
        public long level;
        public long levelPoints;
    }

    [NonSerialized]
    public Save save;

    public Vector3 currencySpawnOffset;

    public UnityEvent moneyChanged;
    public UnityEvent xpChanged;
    public UnityEvent levelChanged;

    protected override void Awake()
    {
        base.Awake();
        Init.LoadComplete += LoadComplete;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Init.LoadComplete -= LoadComplete;
    }

    void LoadComplete()
    {
        moneyChanged.Invoke();
        xpChanged.Invoke();
    }

    public void MachineSellerSellItem(ItemInfo itemInfo, int count, Vector3 position)
    {
        long value = itemInfo.value * count;
        save.money += value;

        moneyChanged.Invoke();

        CurrencyMoney currency = ObjectPooler.instance.Get<CurrencyMoney>();
        currency.Initialize(position);

        CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.MachineSellerRevenue, itemInfo);
        Analytics.instance.NewCurrencyEvent(currencyEvent, value);
    }

    public void MachinePurchaserBuyItem(ItemInfo itemInfo, int count)
    {
        long value = itemInfo.value * count;
        save.money -= value;

        moneyChanged.Invoke();

        CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Sink, CurrencyType.Money, CurrencyEventType.MachinePurchaserFees, itemInfo);
        Analytics.instance.NewCurrencyEvent(currencyEvent, value);
    }

    public void RefundInventory(Inventory inventory)
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

    public void RefundItem(ItemInfo itemInfo, int count)
    {
        long value = itemInfo.value * count;
        save.money += value;

        CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ItemRefunded, itemInfo);
        Analytics.instance.NewCurrencyEvent(currencyEvent, value);

        moneyChanged.Invoke();
    }

    public void ProgressionReward(ProgressionInfo progressionInfo)
    {
        if(progressionInfo.moneyReward > 0)
        {
            long value = progressionInfo.moneyReward;
            save.money += value;

            CurrencyEvent currencyEvent = new CurrencyEvent(GAResourceFlowType.Source, CurrencyType.Money, CurrencyEventType.ProgressionReward, progressionInfo);
            Analytics.instance.NewCurrencyEvent(currencyEvent, value);

            moneyChanged.Invoke();
        }
        
        if(progressionInfo.xpReward > 0)
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
        long value = itemInfo.value * count;
        return save.money >= value;
    }

    public void SetMoney(long value)
    {
        save.money = value;
        moneyChanged.Invoke();
    }
}