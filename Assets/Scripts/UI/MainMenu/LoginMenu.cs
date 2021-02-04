using UnityEngine;

public sealed class LoginMenu : MonoBehaviour
{
    public UILogin loginMenu;

    void Awake()
    {
        Events.MenuStateChanged += MenuStateChanged;
    }

    private void OnDestroy()
    {
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        loginMenu.SetVisible(menuState == MenuState.LoginMenu);
    }
}
