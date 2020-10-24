using UnityEngine;

public sealed class MainMenu : MonoBehaviour
{
    UIMessageBox gui;

    void Awake()
    {
        gui = UIMessageBox.MainMenu(transform);
        gui.open = false;
        Events.MenuStateChanged += MenuStateChanged;
    }

    private void OnDestroy()
    {
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        gui.open = menuState == MenuState.MainMenu;
    }
}
