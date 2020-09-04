#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
class EditorAutoSave
{
    static bool autoSaveEnabled = true;
    static DateTime lastAutoSave = DateTime.UtcNow;
    static bool messageLogged = false;

    static EditorAutoSave()
    {
        EditorApplication.update += () =>
        {
            if (autoSaveEnabled)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (lastAutoSave + TimeSpan.FromMinutes(5) < DateTime.UtcNow)
                    {
                        if (!messageLogged)
                        {
                            messageLogged = true;
                            Debug.Log("Editor auto save enabled");
                        }
                        lastAutoSave = DateTime.UtcNow + TimeSpan.FromMinutes(5);
                        EditorSceneManager.SaveOpenScenes();
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        };

        EditorApplication.playModeStateChanged += PlayModeStateChange =>
        {
            lastAutoSave = DateTime.UtcNow + TimeSpan.FromMinutes(5);
        };
    }
}
#endif