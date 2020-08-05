#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace MultiBuild
{
    // Our own enumeration of targets so we can serialize with confidence
    // in case Unity changes the values of their internal targets
    public enum TargetType
    {
        Win32 = 0,
        Win64 = 1,
        Mac32 = 2,
        Mac64 = 3,
        Mac = 3, // now the default
        MacUniversal = 4,
        Linux64 = 6,
        iOS = 7,
        Android = 8,
        WebGL = 9,
        WinStore = 10,
        PS4 = 12,
        XboxOne = 13,
        SamsungTV = 14,
        tvOS = 17,
        Switch = 18,
    }

    [Serializable]
    public class Target
    {
        public string outputFolder;
        public TargetType targetType;
    }

    public sealed class Settings : ScriptableObject
    {

        public string outputFolder;
        public bool developmentBuild;
        public List<Target> targets;

        public void Reset()
        {
            outputFolder = Directory.GetParent(Application.dataPath).FullName;
            developmentBuild = false;
            targets = new List<Target>();
        }
    }
}
#endif