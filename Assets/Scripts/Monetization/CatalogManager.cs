using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CatalogManager : Singleton<CatalogManager>
{
    static readonly TimeSpan maxTrackingTime = TimeSpan.FromHours(48);

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.LoadComplete += ConfirmAllPurchases;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveLoad.LoadComplete -= ConfirmAllPurchases;
    }
    
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            ConfirmPurchasesThisSession();
        }
    }

    public PlayfabPurchaser Purchase(params CatalogItemInfo[] catalogItemInfos)
    {
        var purchaser = new PlayfabPurchaser();
        purchaser.StartPurchase(catalogItemInfos, result => StartPurchaseResult(purchaser), error => Error(purchaser));
        return purchaser;
    }

    public PlayfabPurchaser ResumeConfirmingPurchase(string orderId, Action<PlayfabPurchaser> onResult, Action<PlayfabPurchaser> onError)
    {
        var purchaser = new PlayfabPurchaser();
        purchaser.ResumeConfirmingPurchase(orderId, result => { ConfirmPurchaseResult(purchaser); onResult(purchaser); }, error => { Error(purchaser); onError(purchaser); });
        return purchaser;
    }

    void StartPurchaseResult(PlayfabPurchaser purchaser)
    {
        PurchaseTracker.instance.Add(purchaser);
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
            ConfirmPurchase(purchaser);
        }
    }

    void ConfirmPurchase(PlayfabPurchaser purchaser)
    {
        purchaser.ConfirmPurchase(result => ConfirmPurchaseResult(purchaser), error => Error(purchaser));
    }

    void ConfirmPurchaseResult(PlayfabPurchaser playfabPurchaser)
    {
        ApplyConfirmationResult(playfabPurchaser);
    }

    void Error(PlayfabPurchaser purchaser)
    {
        if (purchaser.error.ErrorAwaitingPayPalConfirmationPage())
        {

        }
        else
        {
            Debug.LogError("Unkown purchase error\n" + purchaser.error.GenerateErrorReport());
        }
    }

    public void ApplyConfirmationResult(PlayfabPurchaser purchaser)
    {
        if (purchaser.confirmPurchaseResult != null)
        {
            PurchaseTracker.instance.Remove(purchaser.orderId);
            ApplyItemsToGame(purchaser.confirmPurchaseResult.Items);
        }
    }

    void ApplyItemsToGame(List<ItemInstance> items)
    {
        for (int i = 0, len = items.Count; i < len; ++i)
        {
            ItemInstance item = items[i];
            Debug.Log("Applying item " + item.ToJson());
        }
    }

    public void ConfirmAllPurchases()
    {
        List<TrackablePurchase> purchases = PurchaseTracker.instance.TryGetAll();

        if (purchases != null)
        {
            Debug.Log("Confirming all purchases.");

            DateTime trackingCutOffUTC = DateTime.UtcNow - maxTrackingTime;

            for (int i = 0, len = purchases.Count; i < len; ++i)
            {
                TrackablePurchase purchaseTracker = purchases[i];
                PlayfabPurchaser purchaser = ResumeConfirmingPurchase(purchaseTracker.orderId, purchaser =>
                {

                },
                onError =>
                {
                    if (purchaseTracker.trackingStartDateUTC < trackingCutOffUTC)
                    {
                        Debug.LogWarning("No longer tracking order Id after " + maxTrackingTime.TotalHours + " hours, " + purchaseTracker.orderId);
                        PurchaseTracker.instance.Remove(purchaseTracker.orderId);
                    }
                });
            }
        }
    }

    public void ConfirmPurchasesThisSession()
    {
        List<TrackablePurchase> purchases = PurchaseTracker.instance.TryGetConfirmableThisSession();

        if (purchases != null)
        {
            Debug.Log("Confirming purchases that began this session.");

            for (int i = 0, len = purchases.Count; i < len; ++i)
            {
                ConfirmPurchase(purchases[i].purchaser);
            }
        }
    }
}
