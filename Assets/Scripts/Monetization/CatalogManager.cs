using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatalogManager : Singleton<CatalogManager>
{
    public struct Save
    {
        public List<string> confirmableOrderIds;
        public List<string> confirmedOrderIds;
    }

    public Save save = new Save
    {
        confirmableOrderIds = new List<string>()
    };

    List<PlayfabPurchaser> purchasers = new List<PlayfabPurchaser>();

    public PlayfabPurchaser Purchase(params CatalogItemInfo[] catalogItemInfos)
    {
        var purchaser = new PlayfabPurchaser();
        purchaser.StartPurchase(catalogItemInfos, result => StartPurchaseResult(purchaser), error => Error(purchaser));
        purchasers.Add(purchaser);
        return purchaser;
    }

    void StartPurchaseResult(PlayfabPurchaser purchaser)
    {
        save.confirmableOrderIds.Add(purchaser.startPurchaseResult.OrderId);
        SaveLoad.Save();

        purchaser.PayForPurchase(result => PayForPurchaserResult(purchaser), error => Error(purchaser));
    }

    void PayForPurchaserResult(PlayfabPurchaser purchaser)
    {
        if (purchaser.IsAwaitingConfirmationPage())
        {
            purchaser.OpenConfirmationPage();
        }
        else
        {
            purchaser.ConfirmPurchase(result => ConfirmPurchaseResult(purchaser), error => Error(purchaser));
        }
    }

    void ConfirmPurchaseResult(PlayfabPurchaser playfabPurchaser)
    {
        ApplyConfirmationResult(playfabPurchaser);
    }

    void Error(PlayfabPurchaser purchaser)
    {

    }

    public void ApplyConfirmationResult(PlayfabPurchaser purchaser)
    {
        if (purchaser.confirmPurchaseResult != null)
        {
            purchasers.Remove(purchaser);
            save.confirmableOrderIds.Remove(purchaser.confirmPurchaseResult.OrderId);
            if (!save.confirmedOrderIds.Contains(purchaser.confirmPurchaseResult.OrderId))
            {
                save.confirmedOrderIds.Add(purchaser.confirmPurchaseResult.OrderId);

                List<ItemInstance> items = purchaser.confirmPurchaseResult.Items;
                for (int i = 0, len = items.Count; i < len; ++i)
                {
                    ApplyItemToGame(items[i]);
                }
            }
        }
    }

    static void ApplyItemToGame(ItemInstance itemInstance)
    {
        Debug.Log("Applying item " + itemInstance.ToJson());

    }

    public List<PlayfabPurchaser> GetAwaitingConfirmation()
    {
        return purchasers.Where(purchaser => purchaser.state == PlayfabPurchaserState.PayForPurchaseSuccess).ToList();
    }
}
