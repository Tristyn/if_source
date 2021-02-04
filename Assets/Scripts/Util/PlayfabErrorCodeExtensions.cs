
using PlayFab;
using System.Collections.Generic;

public static class PlayfabErrorCodeExtensions
{
    public static bool InvalidLoginCredentials(this PlayFabErrorCode error)
    {
        switch (error)
        {
            case PlayFabErrorCode.InvalidAccount:
            case PlayFabErrorCode.InvalidEmailAddress:
            case PlayFabErrorCode.InvalidEmailOrPassword:
            case PlayFabErrorCode.InvalidPassword:
            case PlayFabErrorCode.InvalidUsername:
            case PlayFabErrorCode.InvalidUsernameOrPassword:
            case PlayFabErrorCode.InvalidParams:
                return true;
        }
        return false;
    }

    public static bool ErrorAwaitingPayPalConfirmationPage(this PlayFabError error)
    {
        if (error.ErrorDetails.TryGetValue("ProviderErrorDetails", out List<string> errorDetails))
        {
            for(int i = 0, len = errorDetails.Count; i < len; ++i)
            {
                if(errorDetails[i].Contains(@"Checkout PayerID is missing."))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
