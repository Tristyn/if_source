using UnityEngine;

public sealed class LoginMenu : MonoBehaviour
{
    public UILogin loginMenu;

    void Awake()
    {
        loginMenu.buttonCancel.onClick.AddListener(OnCancelClicked);
        Events.MenuStateChanged += MenuStateChanged;
        Events.LoginChanged += LoginChanged;
    }

    private void OnDestroy()
    {
        loginMenu.buttonCancel.onClick.RemoveListener(OnCancelClicked);
        Events.MenuStateChanged -= MenuStateChanged;
        Events.LoginChanged -= LoginChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        loginMenu.SetVisible(menuState == MenuState.LoginMenu);
    }

    void LoginChanged(LoginState loginState)
    {
        if (loginState.playfabLoginState == PlayfabLoginState.LoggedIn)
        {
            MenuController.instance.Pop(MenuState.LoginMenu);
        }
    }

    void OnCancelClicked()
    {
        MenuController.instance.Pop(MenuState.LoginMenu);
    }
}
