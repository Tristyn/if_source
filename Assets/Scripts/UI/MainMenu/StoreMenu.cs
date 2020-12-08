using UnityEngine;

public sealed class StoreMenu : MonoBehaviour
{
    public UIStore storeMenu;

    void Awake()
    {
        storeMenu.buttonCancel.onClick.AddListener(OnCancelClicked);
        Events.MenuStateChanged += MenuStateChanged;
    }

    private void OnDestroy()
    {
        storeMenu.buttonCancel.onClick.RemoveListener(OnCancelClicked);
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        storeMenu.SetVisible(menuState == MenuState.StoreMenu);
    }

    void OnCancelClicked()
    {
        MenuController.instance.Pop(MenuState.StoreMenu);
    }
}
