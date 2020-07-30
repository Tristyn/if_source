using System;
using UnityEngine;
using UnityEngine.Events;

public enum CurrencyType
{
    Money,
    Xp
}

public sealed class CurrencySystem : Singleton<CurrencySystem>
{
    [Serializable]
    public struct Save
    {
        public int money;
        public int xp;
        public int level;
        public int levelPoints;
    }

    [NonSerialized]
    public Save save;

    public Vector3 currencySpawnOffset;

    public UnityEvent moneyChanged;
    public UnityEvent xpChanged;

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

    public void ItemSold(ItemInfo itemInfo, int count, Vector3 position)
    {
        int value = itemInfo.value * count;
        save.money += value;

        CurrencyMoney currency = ObjectPooler.instance.Get<CurrencyMoney>();
        currency.Initialize(position);

        moneyChanged.Invoke();
    }

    public bool CanPurchaseItem(ItemInfo itemInfo, int count)
    {
        int value = itemInfo.value * count;
        return save.money >= value;
    }

    public void ItemPurchased(ItemInfo itemInfo, int count)
    {
        int value = itemInfo.value * count;
        save.money -= value;

        moneyChanged.Invoke();
    }

    public void SetMoney(int value)
    {
        save.money = value;
        moneyChanged.Invoke();
    }
}