using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

public enum PlayfabLoginState
{
    None,
    NoCredentials,
    LoggedOut,
    LoggingIn,
    LoggedIn,
    Error
}

public struct LoginState
{
    public PlayfabLoginState playfabLoginState;
    public PlayFabError error;
    public bool newlyCreated;
}

public class PlayFabLogin : Singleton<PlayFabLogin>
{
    public struct Profile
    {
        public string EmailAddress;
        public string PasswordHash;
    }
    public Profile profile;
    public LoginState loginState;

    public void Login()
    {
        if (!HasCredentials())
        {
            Debug.Log("Can't log in, no credentials");
            if (loginState.playfabLoginState != PlayfabLoginState.NoCredentials)
            {
                SetState(PlayfabLoginState.NoCredentials);
            }
            return;
        }
        DoLogin();
    }

    void DoLogin()
    {
        SetState(PlayfabLoginState.LoggingIn);
#if UNITY_ANDROID && !UNITY_EDITOR
        DoLoginAndroid();
#else
        DoLoginEmail();
#endif
    }

    void DoLoginAndroid()
    {
        Debug.Log("Logging in with android");
        var request = new LoginWithAndroidDeviceIDRequest { CreateAccount = true };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
    }

    void DoLoginEmail()
    {
        Debug.Log("Logging in with email " + profile.EmailAddress);
        var request = new LoginWithEmailAddressRequest { Email = profile.EmailAddress, Password = profile.PasswordHash };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public void Register()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        DoLoginAndroid();
#else
        DoRegister();
#endif
    }

    void DoRegister()
    {
        Debug.Log("Registering with email");
        var request = new RegisterPlayFabUserRequest { Email = profile.EmailAddress, Password = profile.PasswordHash, RequireBothUsernameAndEmail = false };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnLoginFailure);
    }

    public bool IsLoggedIn()
    {
        return PlayFabClientAPI.IsClientLoggedIn();
    }

    bool HasCredentials()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return !string.IsNullOrWhiteSpace(profile.EmailAddress) || !string.IsNullOrWhiteSpace(profile.PasswordHash);
#endif
    }

    public void SetCredentials(string emailAddress, string password)
    {
        Debug.Log("Credentials set, email is " + emailAddress);
        profile.EmailAddress = emailAddress;
        if (string.IsNullOrWhiteSpace(password))
        {
            profile.PasswordHash = "";
        }
        else
        {
            profile.PasswordHash = Hasher.Hash(password, emailAddress);
        }
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login success");
        ProfileLoader.Save();
        SetState(PlayfabLoginState.LoggedIn, null, result.NewlyCreated);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Register success");
        ProfileLoader.Save();
        SetState(PlayfabLoginState.LoggedIn, null, true);
    }

    void OnLoginFailure(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.AccountNotFound)
        {
            // Auto register
            Debug.Log("Account not found, registering.");
            Register();
            return;
        }

        Debug.LogWarning("Login failure! Code " + error.Error);
        Debug.LogWarning(error.GenerateErrorReport());
        SetState(PlayfabLoginState.Error, error, false);
    }

    void SetState(PlayfabLoginState playfabLoginState, PlayFabError playFabError = null, bool newlyCreated = false)
    {
        loginState.playfabLoginState = playfabLoginState;
        loginState.error = playFabError;
        loginState.newlyCreated = newlyCreated;
        Events.LoginChanged?.Invoke(loginState);
    }
}
