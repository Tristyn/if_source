
public struct Ownable
{
    public bool owned;
    public CurrencyOffer offer;

    public void Buy()
    {
        if (!owned)
        {
            if (offer.CanBuy())
            {
                owned = true;
                offer.Buy();
            }
        }
    }
}
