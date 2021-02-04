using UnityEngine;

public sealed class AutoSaveCredentials : MonoBehaviour
{
    void Awake()
    {
        Events.LoginChanged += OnLoginChanged;
    }

    void OnDestroy()
    {
        Events.LoginChanged -= OnLoginChanged;
    }

    void OnLoginChanged(LoginState loginState)
    {
        if(loginState.playfabLoginState == PlayfabLoginState.LoggedIn)
        {
            ProfileLoader.Save();
        }
    }
}
