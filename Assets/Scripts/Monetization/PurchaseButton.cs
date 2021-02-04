using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public CatalogItemInfo catalogItemInfo;
    PlayfabPurchaser playfabPurchaser;

    LoginWizard loginWizard = new LoginWizard();

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        loginWizard.Dispose();
    }

    void OnClick()
    {
        loginWizard.Login(false,
            result: () =>
            {
                if (playfabPurchaser == null || playfabPurchaser.error != null)
                {
                    playfabPurchaser = CatalogManager.instance.Purchase(catalogItemInfo);
                }
            },
            error: () =>
            {

            });
    }
}
