using UnityEngine;
using System;
using UnityEngine.Events;

public sealed class UIMessageBoxAction
{
    public string text;
    public Action action;
    public UnityEvent actions;
    public bool enabled = true;
    public bool close = false;
}

public sealed class UIMessageBox : MonoBehaviour
{
    Rect m_windowRect;
    UIMessageBoxAction[] actions;
    public string title;
    public string msg;
    public int maxWidth = 640;
    public int maxHeight = 480;
    GUI.WindowFunction windowFunc;

    public static UIMessageBox MainMenu(Transform parent)
    {
        return MessageBox(parent, "Idle Factory", "Main Menu", new UIMessageBoxAction[]{
            new UIMessageBoxAction
            {
                text="Resume",
                action = () => MenuController.instance.Pop(MenuState.MainMenu)
            },
            new UIMessageBoxAction
            {
                text="Store",
                action = () => MenuController.instance.Push(MenuState.StoreMenu)
            },
            new UIMessageBoxAction
            {
                text="New Game",
                action = () => MenuController.instance.Push(MenuState.NewGameMenu)
            },
            new UIMessageBoxAction
            {
                text="Export",
                action = () => MenuController.instance.Push(MenuState.SavesMenu)
            }
        });
    }

    public static UIMessageBox NewGameMenu(Transform parent)
    {
        return MessageBox(parent, "Idle Factory", "New freeplay game?", new UIMessageBoxAction[]{
            new UIMessageBoxAction
            {
                text="Freeplay",
                action = GameModeInitializer.InitializeFreePlay
            },
            new UIMessageBoxAction
            {
                text="Sandbox",
                action = GameModeInitializer.InitializeSandbox
            },
#if UNITY_EDITOR
            new UIMessageBoxAction
            {
                text = "Puzzles",
                action = GameModeInitializer.InitializePuzzles
            },
#endif
            new UIMessageBoxAction
            {
                text="Back",
                action = () => MenuController.instance.Pop(MenuState.NewGameMenu)
            }
        });
    }

    public static UIMessageBox SavesMenu(Transform parent)
    {
        return MessageBox(parent, "Idle Factory", "Import and export the save game.", new UIMessageBoxAction[]{
            new UIMessageBoxAction
            {
                text="Export to Clipboard",
                action = SaveLoadClipboard.SaveToClipboard
            },
            new UIMessageBoxAction
            {
                text="Import from Clipboard",
                action = SaveLoadClipboard.LoadFromClipboard
            },
            new UIMessageBoxAction
            {
                text="Back",
                action = () => MenuController.instance.Pop(MenuState.SavesMenu)
            }
        });
    }

    public static UIMessageBox MessageBox(Transform parent, string title, string msg, UIMessageBoxAction[] actions)
    {
        GameObject go = new GameObject("UIMessageBox ");
        go.transform.SetParent(parent, worldPositionStays: false);
        UIMessageBox dlg = go.AddComponent<UIMessageBox>();
        dlg.Init(title, msg, actions);
        return dlg;
    }

    void Init(string title, string msg, UIMessageBoxAction[] actions)
    {
        gameObject.name += "(" + title + ")";
        this.title = title;
        this.msg = msg;
        this.actions = actions;
        windowFunc = WindowFunc; // Cache this delegate that Unity asks for every frame
        open = true;
    }

    void ExecuteAction(UIMessageBoxAction action)
    {
        open = !action.close;
        action.action?.Invoke();
        action.actions?.Invoke();
    }

    public bool open
    {
        get
        {
            return enabled;
        }
        set
        {
            enabled = value;
        }
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    void OnGUI()
    {
        int width = Mathf.Min(maxWidth, Screen.width - 80);
        int height = Mathf.Min(maxHeight, Screen.height - 80);
        m_windowRect = new Rect(
            (Screen.width - width) / 2,
            (Screen.height - height) / 2,
            width,
            height);

        m_windowRect = GUI.Window(0, m_windowRect, windowFunc, title);
    }

    void WindowFunc(int windowID)
    {
        const int border = 15;
        const int width = 200;
        const int height = 30;
        const int spacing = 10;
        
        Rect msgRect = new Rect(
            border,
            border + spacing,
            m_windowRect.width - border * 2,
            m_windowRect.height - border * 2 - height - spacing);
        GUI.Label(msgRect, msg);
        
        float allButtonsHeight = (height + border) * actions.Length;
        float allButtonsStartY = (m_windowRect.height - allButtonsHeight) / 2;
        float currentY = allButtonsStartY;

        for (int i = 0, len = actions.Length; i < len; ++i)
        {
            currentY += border + height;
            Rect actionRect = new Rect(
                (m_windowRect.width - width) / 2,
                currentY,
                width,
                height);

            UIMessageBoxAction action = actions[i];
            if (GUI.Button(actionRect, action.text))
            {
                ExecuteAction(action);
            }
        }
    }
}
