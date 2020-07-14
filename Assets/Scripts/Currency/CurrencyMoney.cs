using UnityEngine;

public sealed class CurrencyMoney : Currency
{
    public CurrencyMoney()
    {
        currencyType = CurrencyType.Money;
    }

    public override void Completed()
    {
        ;
    }
}
