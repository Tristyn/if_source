using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum MenuState
{
    None = 0,
    MainMenu = 1,
    NewGameMenu = 2,
    SavesMenu = 3,
    LoginMenu = 4,
    StoreMenu = 5,
}

public sealed class MenuController : Singleton<MenuController>
{
    Stack<MenuState> menuNavigation = new Stack<MenuState>();

    public MenuState menuState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return menuNavigation.Count > 0 ? menuNavigation.Peek() : MenuState.None;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.PostLoad += PostLoad;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveLoad.PostLoad -= PostLoad;
    }

    public void PostLoad()
    {
        menuNavigation.Clear();
        Events.MenuStateChanged?.Invoke(menuState);
    }

    public void Push(MenuState menuState)
    {
        menuNavigation.Push(menuState);
        Events.MenuStateChanged?.Invoke(menuState);
    }

    public void Pop(MenuState menuState)
    {
        if (menuState == this.menuState)
        {
            menuNavigation.Pop();
            Events.MenuStateChanged?.Invoke(this.menuState);
        }
    }
}
