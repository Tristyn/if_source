using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayfabPurchaserState
{
    None = 0,

    // Design time errors
    MissingPaymentOptionError = 2,

    Started = 3,
    StartedSuccess = 4,
    PayForPurchaseStarted = 5,
    ConfirmPurchase = 7,
    ConfirmPurchaseSuccess = 8,

    StartedError = 9,
    PayForPurchaseError = 10,
    ConfirmPurchaseError = 11,
}

public class PlayfabPurchaser
{
    public string orderId;
    public PlayfabPurchaserState state;
    [NonSerialized]
    public StartPurchaseResult startPurchaseResult;
    [NonSerialized]
    public PayForPurchaseResult payForPurchaseResult;
    [NonSerialized]
    public ConfirmPurchaseResult confirmPurchaseResult;
    [NonSerialized]
    public PlayFabError error;
    public CatalogItemInfo[] catalogItemInfos;

    const string PAYPAL_PROVIDER = "PayPal";

    public bool CanStartPurchase()
    {
        return state == PlayfabPurchaserState.None;
    }

    public void StartPurchase(CatalogItemInfo[] catalogItemInfos, Action<StartPurchaseResult> onResult, Action<PlayFabError> onError)
    {
        if (!CanStartPurchase())
        {
            Debug.LogWarning("Tried starting purchase when state was " + state);
            return;
        }

        if (catalogItemInfos.Length == 1)
        {
            Debug.Log("Starting purchase " + catalogItemInfos[0].itemId);
        }
        else
        {
            Debug.Log("Starting purchase of " + catalogItemInfos.Length + " items");
        }

        state = PlayfabPurchaserState.Started;
        this.catalogItemInfos = catalogItemInfos;
        List<ItemPurchaseRequest> items = catalogItemInfos.Select(catalogItemInfo =>
            new ItemPurchaseRequest()
            {
                ItemId = catalogItemInfo.itemId,
                Quantity = catalogItemInfo.quantity,
                Annotation = "Purchased via in-game store"
            }
        ).ToList();

        string catalogVersion = catalogItemInfos.Select(catalogItemInfo => catalogItemInfo.catalogVersion).FirstOrDefault();

        PlayFabClientAPI.StartPurchase(new StartPurchaseRequest()
        {
            CatalogVersion = catalogVersion,
            Items = items
        },
        result =>
        {
            startPurchaseResult = result;
            state = PlayfabPurchaserState.StartedSuccess;
            orderId = result.OrderId;
            Debug.Log("Start purchase success, order Id" + result.OrderId);
            onResult(result);
        },
        error =>
        {
            this.error = error;
            state = PlayfabPurchaserState.StartedError;
            Debug.Log("Start purchase error\n" + error.GenerateErrorReport());
            onError(error);
        });
    }

    public bool CanPayForPurchase()
    {
        return state == PlayfabPurchaserState.StartedSuccess && startPurchaseResult != null;
    }

    public void PayForPurchase(Action<PayForPurchaseResult> onResult, Action<PlayFabError> onError)
    {
        if (!CanPayForPurchase())
        {
            Debug.LogWarning("Tried paying for purchase when state was " + state);
            return;
        }

        PaymentOption paypalPaymentOption = null;
        for (int i = 0, len = startPurchaseResult.PaymentOptions.Count; i < len; ++i)
        {
            if (startPurchaseResult.PaymentOptions[i].ProviderName == PAYPAL_PROVIDER)
            {
                paypalPaymentOption = startPurchaseResult.PaymentOptions[i];
                break;
            }
        }

        if (paypalPaymentOption == null)
        {
            state = PlayfabPurchaserState.MissingPaymentOptionError;
            return;
        }

        PlayFabClientAPI.PayForPurchase(new PayForPurchaseRequest()
        {
            OrderId = startPurchaseResult.OrderId,
            ProviderName = paypalPaymentOption.ProviderName,
            Currency = paypalPaymentOption.Currency
        },
        result =>
        {
            payForPurchaseResult = result;
            state = PlayfabPurchaserState.ConfirmPurchase;
            orderId = result.OrderId;
            Debug.Log("Pay for purchase success, order Id" + result.OrderId);
            onResult(result);
        },
        error =>
        {
            this.error = error;
            state = PlayfabPurchaserState.PayForPurchaseError;
            Debug.Log("Start purchase error\n" + error.GenerateErrorReport());
            onError(error);
        });
    }

    public bool IsAwaitingConfirmationPage()
    {
        return state == PlayfabPurchaserState.ConfirmPurchase && payForPurchaseResult != null && !string.IsNullOrEmpty(payForPurchaseResult.PurchaseConfirmationPageURL);
    }

    public void OpenConfirmationPage()
    {
        if (IsAwaitingConfirmationPage())
        {
            Application.OpenURL(payForPurchaseResult.PurchaseConfirmationPageURL);
        }
    }

    public bool CanConfirmPurchase()
    {
        return state == PlayfabPurchaserState.ConfirmPurchase || state == PlayfabPurchaserState.ConfirmPurchaseError;
    }

    public void ConfirmPurchase(Action<ConfirmPurchaseResult> onResult, Action<PlayFabError> onError)
    {
        if (!CanConfirmPurchase())
        {
            return;
        }

        Debug.Log("Confirming purchase, order Id " + orderId);
        PlayFabClientAPI.ConfirmPurchase(new ConfirmPurchaseRequest
        {
            OrderId = orderId
        },
        result =>
        {
            confirmPurchaseResult = result;
            state = PlayfabPurchaserState.ConfirmPurchaseSuccess;
            orderId = result.OrderId;
            Debug.Log("Confirm purchase success, order Id " + result.OrderId);
            onResult(result);
        },
        error =>
        {
            this.error = error;
            state = PlayfabPurchaserState.ConfirmPurchaseError;
            onError(error);
        });
    }
    
    public void ResumeConfirmingPurchase(string orderId, Action<ConfirmPurchaseResult> onResult, Action<PlayFabError> onError)
    {
        this.orderId = orderId;
        state = PlayfabPurchaserState.ConfirmPurchase;
        ConfirmPurchase(onResult, onError);
    }
}
