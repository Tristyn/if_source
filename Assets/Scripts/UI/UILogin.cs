using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    public TextMeshProUGUI textLoginStatus;
    public TextMeshProUGUI textError;
    public TMP_InputField inputEmailAddress;
    public TMP_InputField inputPassword;

    public Button buttonContinue;
    public Button buttonCancel;

    public Color colorSignedOut;
    public Color colorSigningIn;
    public Color colorSignedIn;

    public bool canContinue;

    UIBehaviour[] uibehaviours;

    void Awake()
    {
        Events.LoginChanged += OnLoginChanged;
        buttonContinue.onClick.AddListener(OnContinueClicked);
        uibehaviours = gameObject.GetComponentsInChildren<UIBehaviour>();
        SetVisible(false);
    }

    void OnDestroy()
    {
        Events.LoginChanged -= OnLoginChanged;
        buttonContinue.onClick.RemoveListener(OnContinueClicked);
    }

    public void SetVisible(bool visible)
    {
        uibehaviours.SetEnabled(visible);

        if (visible)
        {
            inputEmailAddress.text = PlayFabLogin.instance.profile.EmailAddress;
            OnLoginChanged(PlayFabLogin.instance.loginState);
        }
    }

    void OnContinueClicked()
    {
        PlayFabLogin.instance.SetCredentials(inputEmailAddress.text, inputPassword.text);
        PlayFabLogin.instance.Login();
    }

    void OnLoginChanged(LoginState loginState)
    {
        switch (loginState.playfabLoginState)
        {
            case PlayfabLoginState.LoggingIn:
                canContinue = false;
                break;
            case PlayfabLoginState.None:
            case PlayfabLoginState.NoCredentials:
            case PlayfabLoginState.LoggedOut:
            case PlayfabLoginState.LoggedIn:
            case PlayfabLoginState.Error:
            default:
                canContinue = true;
                break;
        }

        switch (loginState.playfabLoginState)
        {
            case PlayfabLoginState.NoCredentials:
            case PlayfabLoginState.LoggedOut:
            case PlayfabLoginState.Error:
                textLoginStatus.text = "● Not signed in";
                textLoginStatus.color = colorSignedOut;
                break;
            case PlayfabLoginState.LoggingIn:
                textLoginStatus.text = "● Signing in";
                textLoginStatus.color = colorSigningIn;
                break;
            case PlayfabLoginState.LoggedIn:
                textLoginStatus.text = "● Signed in";
                textLoginStatus.color = colorSignedIn;
                break;
        }

        switch (loginState.playfabLoginState)
        {
            case PlayfabLoginState.Error:
                textError.text = loginState.error.ErrorMessage;
                break;
            case PlayfabLoginState.None:
            case PlayfabLoginState.NoCredentials:
            case PlayfabLoginState.LoggedOut:
            case PlayfabLoginState.LoggingIn:
            case PlayfabLoginState.LoggedIn:
            default:
                textError.text = "";
                break;
        }
    }
}
