#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* Easiest way to check if we're running 5.4 or lower. */
#if UNITY_5_5_OR_NEWER
#else
namespace UnityEditor
{
	public struct BuildPlayerOptions
	{
		public string[] scenes { get; set; }
		public string locationPathName { get; set; }
		public string assetBundleManifestPath { get; set; }
		public BuildTargetGroup targetGroup { get; set; }
		public BuildTarget target { get; set; }
		public BuildOptions options { get; set; }
	}
}
#endif

namespace MultiBuild
{
    public sealed class SettingsWindow : EditorWindow
    {

        // Manually format the descriptive names
        // Simpler than DescriptionAttribute style IMO
        static Dictionary<TargetType, string> _targetNames;
        public static Dictionary<TargetType, string> TargetNames
        {
            get
            {
                if (_targetNames == null)
                {
                    _targetNames = new Dictionary<TargetType, string> {
                        {TargetType.Android, "Android"},
                        {TargetType.iOS, "iOS"},
                        {TargetType.Linux64, "Linux 64-bit"},
                        {TargetType.Mac32, "Mac 32-bit"},
                        {TargetType.Mac64, "Mac 64-bit"},
                        {TargetType.MacUniversal, "Mac Universal"},
                        {TargetType.WebGL, "WebGL"},
                        {TargetType.Win32, "Windows 32-bit"},
                        {TargetType.Win64, "Windows 64-bit"},
                        {TargetType.WinStore, "Windows Store App"},
                        {TargetType.PS4, "Playstation 4"},
                        {TargetType.XboxOne, "Xbox One"},
                        {TargetType.SamsungTV, "Samsung TV"},
                        {TargetType.tvOS, "tvOS"},
#if UNITY_5_6_OR_NEWER
                        {TargetType.Switch, "Nintendo Switch"},
#endif
                    };
                }
                return _targetNames;
            }
        }

        // Because we need to sort and Unity Popup doesn't have a data tag
        Dictionary<string, TargetType> _targetNameToValue;
        Dictionary<string, TargetType> TargetNameToValue
        {
            get
            {
                if (_targetNameToValue == null)
                {
                    _targetNameToValue = new Dictionary<string, TargetType>();
                    foreach (var target in TargetNames.Keys)
                    {
                        _targetNameToValue[TargetNames[target]] = target;
                    }
                }
                return _targetNameToValue;
            }
        }



        TargetType[] _targets;
        TargetType[] Targets
        {
            get
            {
                if (_targets == null)
                {
                    _targets = (TargetType[])Enum.GetValues(typeof(TargetType));
                }
                return _targets;
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
        List<string> _targetNamesNotAdded;
        int _targetToAddIndex;

        //[MenuItem("File/MultiBuild...", priority = 205)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SettingsWindow), false, "MultiBuild");
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUI.backgroundColor = new Color(0, 0.6f, 0, 1);
            if (GUILayout.Button("Build Selected Platforms", ActionButtonStyle, GUILayout.MinHeight(30)))
            {
                // do eet
                Build();
            }
        }

        [MenuItem("File/MultiBuild", priority = 205)]
        public static void Build()
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
        }

    }

}
#endif