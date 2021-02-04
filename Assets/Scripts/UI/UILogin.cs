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
        buttonCancel.onClick.AddListener(OnCancelClicked);
        uibehaviours = gameObject.GetComponentsInChildren<UIBehaviour>();
        SetVisible(false);
    }

    void OnDestroy()
    {
        Events.LoginChanged -= OnLoginChanged;
        buttonContinue.onClick.RemoveListener(OnContinueClicked);
        buttonCancel.onClick.RemoveListener(OnCancelClicked);
    }

    public void SetVisible(bool visible)
    {
        uibehaviours.SetEnabled(visible);

        if (visible)
        {
            inputEmailAddress.text = PlayFabLogin.instance.profile.EmailAddress;
            SetState(PlayFabLogin.instance.loginState);
        }
        else
        {
            Events.LoginMenuClosed?.Invoke();
        }
    }

    void OnContinueClicked()
    {
        PlayFabLogin.instance.SetCredentials(inputEmailAddress.text, inputPassword.text);
        PlayFabLogin.instance.Login();
    }

    void OnCancelClicked()
    {
        MenuController.instance.Pop(MenuState.LoginMenu);
    }

    void OnLoginChanged(LoginState loginState)
    {
        SetState(loginState);

        if (loginState.playfabLoginState == PlayfabLoginState.LoggedIn)
        {
            MenuController.instance.Pop(MenuState.LoginMenu);
        }
    }

    void SetState(LoginState loginState)
    {
        canContinue = loginState.playfabLoginState switch
        {
            PlayfabLoginState.LoggingIn => false,
            _ => true,
        };

        textError.text = loginState.playfabLoginState switch
        {
            PlayfabLoginState.Error => loginState.error.ErrorMessage,
            _ => "",
        };

        switch (loginState.playfabLoginState)
        {
            case PlayfabLoginState.None:
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
    }
}
