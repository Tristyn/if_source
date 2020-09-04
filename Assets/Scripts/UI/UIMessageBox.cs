using UnityEngine;
using System;

// example:
// UIMessageBox.MessageBox("error", "Sorry but you're S.O.L", () => { Application.Quit() });

public struct UIMessageBoxAction
{
    public string text;
    public Action action;
    public bool disabled;
}

public class UIMessageBox : MonoBehaviour
{
    Rect m_windowRect;
    UIMessageBoxAction[] actions;
    string title;
    string msg;
    GUI.WindowFunction windowFunc;

    public static void MessageBox(string title, string msg, UIMessageBoxAction[] actions)
    {
        UIMessageBox dlg = NewMessageBoxGO();
        dlg.Init(title, msg, actions);
    }

    static UIMessageBox NewMessageBoxGO()
    {
        GameObject go = new GameObject("UIMessageBox ");
        return go.AddComponent<UIMessageBox>();
    }

    void Init(string title, string msg, UIMessageBoxAction[] actions)
    {
        gameObject.name += "(" + title + ")";
        this.title = title;
        this.msg = msg;
        this.actions = actions;
        windowFunc = WindowFunc; // Cache this delegate that Unity asks for every frame
    }

    void OnGUI()
    {
        const int maxWidth = 640;
        const int maxHeight = 480;

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
            //Rect actionRect = new Rect(
            //    m_windowRect.width - width - border,
            //    m_windowRect.height - height - border,
            //    width,
            //    height);
            currentY += border + height;
            Rect actionRect = new Rect(
                (m_windowRect.width - width) / 2,
                currentY,
                width,
                height);

            UIMessageBoxAction action = actions[i];
            if (GUI.Button(actionRect, action.text))
            {
                Destroy(gameObject);
                action.action();
            }
        }
    }
}
