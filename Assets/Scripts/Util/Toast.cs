using UnityEngine;

public static class Toast
{
    public static void ShowToast(string message)
    {
        ShowAndroidToast(message);
        ShowWebGLToast(message);
    }

    /// <summary>
    /// Shows a toast message. These can be extremely annoying if overdone. Android show a toast, webgl shows an alert.
    /// </summary>
    /// <param name="message"></param>
    public static void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
#endif
    }

    public static void ShowWebGLToast(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("alert", message);
#endif
    }
}
