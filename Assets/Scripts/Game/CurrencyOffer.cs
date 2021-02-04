public struct CurrencyOffer
{
    public CurrencyEventType currencyEventType;
    public CurrencyType currencyType;
    public long value;

    public bool CanBuy()
    {
        return CurrencySystem.instance.CanBuy(1, value, currencyType);
    }

    public void Buy()
    {
        CurrencySystem.instance.Buy(1, value, currencyType);
    }
}
