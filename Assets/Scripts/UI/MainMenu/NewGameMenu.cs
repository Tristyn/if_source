using UnityEngine;

public sealed class NewGameMenu : MonoBehaviour
{
    UIMessageBox gui;

    void Awake()
    {
        gui = UIMessageBox.NewGameMenu(transform);
        gui.open = false;
        Events.MenuStateChanged += MenuStateChanged;
    }

    void OnDestroy()
    {
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        gui.open = menuState == MenuState.NewGameMenu;
    }
}
