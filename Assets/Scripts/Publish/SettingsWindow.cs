#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace MultiBuild
{
    public sealed class SettingsWindow : EditorWindow
    {
        string _buildNumber;
        string BuildNumber
        {
            get
            {
                if (_buildNumber == null)
                {
                    Version version = new Version(PlayerSettings.bundleVersion);
                    DateTime date = DateTime.Now; // Local date
                    int revision = version.Revision;
                    if (date.Month != version.Minor || date.Day != version.Build)
                    {
                        revision = 0;
                    }

                    version = new Version(version.Major, date.Month, date.Day, revision);
                    _buildNumber = version.ToString();

                    PlayerSettings.bundleVersion = _buildNumber;
                }
                return _buildNumber;
            }
        }

        GUIStyle _actionButtonStyle;
        GUIStyle ActionButtonStyle
        {
            get
            {
                if (_actionButtonStyle == null)
                {
                    _actionButtonStyle = new GUIStyle(GUI.skin.button);
                    _actionButtonStyle.fontStyle = FontStyle.Bold;
                    _actionButtonStyle.normal.textColor = Color.white;
                }
                return _actionButtonStyle;
            }
        }
        GUIStyle _labelMarginStyle;
        GUIStyle LabelMarginStyle
        {
            get
            {
                if (_labelMarginStyle == null)
                {
                    _labelMarginStyle = new GUIStyle();
                    _labelMarginStyle.margin.left = GUI.skin.label.margin.left;
                }
                return _labelMarginStyle;
            }
        }
        GUIStyle _removeButtonContainerStyle;
        GUIStyle RemoveButtonContainerStyle
        {
            get
            {

                if (_removeButtonContainerStyle == null)
                {
                    _removeButtonContainerStyle = new GUIStyle();
                    _removeButtonContainerStyle.margin.left = 30;
                }
                return _removeButtonContainerStyle;
            }
        }

        [MenuItem("File/MultiBuild...", priority = 205)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SettingsWindow), false, "MultiBuild");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Build Number: " + BuildNumber);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUI.backgroundColor = new Color(0, 0.6f, 0, 1);
            if (GUILayout.Button("Build Selected Platforms", ActionButtonStyle, GUILayout.MinHeight(30)))
            {
                bool ok = false;
                try
                {
                    // do eet
                    ok = Build();
                }
                finally
                {
                    if (ok)
                    {
                        Version version = new Version(PlayerSettings.bundleVersion);
                        version = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);
                        _buildNumber = version.ToString();
                        PlayerSettings.bundleVersion = _buildNumber;
                    }
                }
            }
        }

        public static bool Build()
        {
            var savedTarget = EditorUserBuildSettings.activeBuildTarget;
            bool ok = true;
            try
            {
                ok = Builder.Build(Storage.LoadOrCreateSettings(), (opts, progress, done) =>
                {
                    string message = done ?
                        string.Format("Building {0} Done", opts.target.ToString()) :
                        string.Format("Building {0}...", opts.target.ToString());
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Building project...",
                        message,
                        progress))
                    {
                        return false; // cancel
                    }
                    return true;
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Build error", e.Message, "Close");
                ok = false;
            }

            EditorUtility.ClearProgressBar();
            if (ok)
            {
                Debug.Log("MultiBuild succeess");
            }
            else
            {
                EditorUtility.DisplayDialog("Cancelled", "Build cancelled before finishing.", "Close");
            }

            // Building can change the active target, can cause warnings or odd behaviour
            // Put it back to how it was
            if (EditorUserBuildSettings.activeBuildTarget != savedTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(Builder.GroupForTarget(savedTarget), savedTarget);
            }
            return ok;
        }

    }

}
#endif