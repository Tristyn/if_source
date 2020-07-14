using UnityEngine;

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

    
    public void ItemSold(ItemInfo itemInfo, int count, Vector3 position)
    {
        int value = itemInfo.value * count;
        Money += value;

        CurrencyMoney currency = ObjectPooler.instance.Get<CurrencyMoney>();
        currency.Initialize(position);
        currency.Collect();
    }
}