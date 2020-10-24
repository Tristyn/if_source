using UnityEngine;

public class SavesMenu : MonoBehaviour
{
    UIMessageBox gui;

    void Awake()
    {
        gui = UIMessageBox.SavesMenu(transform);
        gui.open = false;
        Events.MenuStateChanged += MenuStateChanged;
    }

    private void OnDestroy()
    {
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        gui.open = menuState == MenuState.SavesMenu;
    }
}
