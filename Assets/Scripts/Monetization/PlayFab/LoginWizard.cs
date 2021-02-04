using System;
using UnityEngine;

public class LoginWizard : IDisposable
{
    Action resultCallbacks;
    Action errorCallbacks;

    bool gettingNewCredentials;

    public LoginWizard()
    {
        Events.LoginChanged += OnLoginChanged;
        Events.LoginMenuClosed += OnLoginMenuClosed;
    }

    public void Login(bool force, Action result, Action error)
    {
        gettingNewCredentials = false;
        if (!force && PlayFabLogin.instance.IsLoggedIn())
        {
            Debug.Log("Logged in already.");
            result?.Invoke();
            return;
        }

        resultCallbacks += result;
        errorCallbacks += error;

        Debug.Log("Initiating log in with existing credentials.");
        PlayFabLogin.instance.Login();
    }

    public void Dispose()
    {
        Events.LoginChanged -= OnLoginChanged;
    }

    void OnLoginChanged(LoginState state)
    {
        switch (state.playfabLoginState)
        {
            case PlayfabLoginState.NoCredentials:
                GetNewCredentailsFromUserOnce();
                break;
            case PlayfabLoginState.LoggedIn:
                Debug.Log("Logged in.");
                resultCallbacks?.Invoke();
                resultCallbacks = null;
                errorCallbacks = null;
                break;
            case PlayfabLoginState.Error:
                if (!state.error.Error.InvalidLoginCredentials())
                {
                    Debug.Log("Log in error. Ending wizard.");
                }
                GetNewCredentailsFromUserOnce();
                break;
            case PlayfabLoginState.LoggedOut:
                Debug.Log("Logged out. Ending wizard.");
                errorCallbacks?.Invoke();
                resultCallbacks = null;
                errorCallbacks = null;
                break;
            case PlayfabLoginState.LoggingIn:
                Debug.Log("Logging in. Awaiting new log in state.");
                break;
            case PlayfabLoginState.None:
                break;
        }
    }

    void GetNewCredentailsFromUserOnce()
    {
        if (gettingNewCredentials)
        {
            if (MenuController.instance.menuState != MenuState.LoginMenu)
            {
                Debug.Log("Login failure and previously attempted to get new credentials. Ending login wizard.");
                errorCallbacks?.Invoke();
                resultCallbacks = null;
                errorCallbacks = null;
            }
        }
        else
        {
            Debug.Log("Opening login menu to get credentials.");
            gettingNewCredentials = true;
            MenuController.instance.Push(MenuState.LoginMenu);
        }
    }

    void OnLoginMenuClosed()
    {
        switch (PlayFabLogin.instance?.loginState.playfabLoginState)
        {
            case PlayfabLoginState.NoCredentials:
                errorCallbacks?.Invoke();
                resultCallbacks = null;
                errorCallbacks = null;
                break;
            case PlayfabLoginState.Error:
                errorCallbacks?.Invoke();
                resultCallbacks = null;
                errorCallbacks = null;
                break;
            case PlayfabLoginState.LoggedIn:
            case PlayfabLoginState.LoggingIn:
            case PlayfabLoginState.LoggedOut:
            case PlayfabLoginState.None:
                break;
        }
    }
}
