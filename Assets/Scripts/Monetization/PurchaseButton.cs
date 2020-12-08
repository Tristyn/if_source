using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public CatalogItemInfo catalogItemInfo;
    PlayfabPurchaser playfabPurchaser;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        PlayFabLogin.instance.Login();
        if (PlayFabLogin.instance.loginState.playfabLoginState == PlayfabLoginState.LoggedIn)
        {
            if (playfabPurchaser == null || playfabPurchaser.error != null)
            {
                playfabPurchaser = CatalogManager.instance.Purchase(catalogItemInfo);
            }
        }
    }
}
