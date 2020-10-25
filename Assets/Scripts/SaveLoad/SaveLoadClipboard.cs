using UnityEngine;

public static class SaveLoadClipboard
{
    public static void SaveToClipboard()
    {
        string json = SaveLoad.BuildSaveJson(new SaveLoad.SaveOptions());
        GUIUtility.systemCopyBuffer = json;
        Toast.ShowAndroidToast("Copied to clipboard.");
    }

    public static void LoadFromClipboard()
    {
        string json = GUIUtility.systemCopyBuffer;
        if (!SaveLoad.LoadFromJson(json))
        {
            Toast.ShowToast("Error while loading.");
        }
    }
}
