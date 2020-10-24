using System;
using System.Runtime.CompilerServices;

public enum MenuState
{
    None = 0,
    Closed = 1,
    MainMenu = 2,
    NewGameMenu = 3,
    SavesMenu
}

public sealed class MenuController : Singleton<MenuController>
{
    public MenuState menuState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
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
        SetState(MenuState.Closed);
    }

    public void SetState(MenuState menuState)
    {
        this.menuState = menuState;
        Events.MenuStateChanged?.Invoke(menuState);
    }
}
