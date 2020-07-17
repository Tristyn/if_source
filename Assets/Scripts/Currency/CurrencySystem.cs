using UnityEngine;
using UnityEngine.Events;

public enum CurrencyType
{
    Money,
    Xp
}

public class CurrencySystem : Singleton<CurrencySystem>
{
    public int Money;
    public int Xp;
    public int Level;
    public int LevelPoints;

    public Vector3 currencySpawnOffset;
    public AnimationCurve heightAnimationCurve;
    public AnimationCurve yawAnimationCurve;
    public float collectAnimationDuration;

    public UnityEvent moneyChanged;
    public UnityEvent xpChanged;

    public void ItemSold(ItemInfo itemInfo, int count, Vector3 position)
    {
        int value = itemInfo.value * count;
        Money += value;

        CurrencyMoney currency = ObjectPooler.instance.Get<CurrencyMoney>();
        currency.Initialize(position);

        moneyChanged.Invoke();
    }
}