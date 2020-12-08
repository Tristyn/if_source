using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuOpener : MonoBehaviour
{
    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        Events.MenuStateChanged += MenuStateChanged;
    }

    private void OnDestroy()
    {
        Events.MenuStateChanged -= MenuStateChanged;
    }

    void MenuStateChanged(MenuState menuState)
    {
        gameObject.SetActive(menuState == MenuState.None);
    }

    void OnClick()
    {
        MenuController.instance.Push(MenuState.MainMenu);
    }
}
