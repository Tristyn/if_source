#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO.Compression;
using UnityEditor.Callbacks;
using UnityEditor.Build.Player;

namespace MultiBuild
{
    public static class Builder
    {

        /// <summary>
        /// Build with default saved options
        /// </summary>
        public static bool Build()
        {
            var settings = Storage.LoadSettings();
            if (settings == null)
            {
                throw new InvalidOperationException("No saved settings found, cannot build");
            }
            return Build(settings, null);
        }

        /// <summary>
        /// Build using command line arguments, i.e. via the -executeMethod argument to Unity.
        /// Unlike Build() this does not load any saved settings. Always uses the product name.
        /// Arguments must all be after the executeMethod call:
        ///  Unity -quit -batchmode -executeMethod MultiBuild.Builder.BuildCommandLine <outputFolder> <is_dev> <targetName> [targetName...]
        /// Products are created in <outputFolder>/targetName/
        /// targetName must match the enum MultiBuild.Target
        /// No other arguments must be after that
        /// </summary>
        /// <returns></returns>
        public static void BuildCommandLine()
        {
            // We get all the args, including UNity.exe, -quit -batchmode etc
            // read everything after our execute call
            var args = System.Environment.GetCommandLineArgs();
            // 0 = looking for args
            // 1 = expecting output folder
            // 2 = expecting dev boolean
            // 3 = expecting target
            int stage = 0;
            Settings settings = new Settings();
            settings.Reset();

            string usage = "\nUsage:\n  Unity <args> -executeMethod MultiBuild.Builder.BuildCommandLine <outputFolder> <is_dev> <targetName> [targetName...]\n";

            for (int i = 0; i < args.Length; ++i)
            {
                switch (stage)
                {
                    case 0:
                        // Skipping over all args until we see ours
                        if (args[i].Equals("MultiBuild.Builder.BuildCommandLine"))
                        {
                            ++stage;
                        }
                        break;
                    case 1:
                        // next arg is output
                        settings.outputFolder = args[i];
                        ++stage;
                        break;
                    case 2:
                        // next arg is dev flag
                        try
                        {
                            settings.developmentBuild = Boolean.Parse(args[i]);
                            ++stage;
                        }
                        catch (FormatException)
                        {
                            throw new ArgumentException("Development build argument was not a valid boolean" + usage);
                        }
                        break;
                    default:
                    case 3:
                        // all subsequent args should be targets
                        try
                        {
                            //Cmdline is broken
                            //settings.targets.Add((TargetType)Enum.Parse(typeof(TargetType), args[i]));
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException(string.Format("Invalid target '{0}'", args[i]));
                        }
                        break;
                }
            }
            if (stage != 3 || settings.targets.Count == 0)
            {
                throw new ArgumentException("Not enough arguments." + usage);
            }

            Build(settings, null);

        }

        /// <summary>
        /// Build with given settings, call back if required
        /// </summary>
        /// <param name="settings">Settings to build with</param>
        /// <param name="callback">Callback which is called before and after a
        /// given build target, being passed the build options, a float from
        /// 0..1 indicating how far through the process we are, and a bool which
        /// is false for the pre-call and true for the post-call. Return true to
        /// continue or false to abort.</param>
        /// <returns>True if the process completed fully or false if was cancelled by callback</returns>
        public static bool Build(Settings settings, System.Func<BuildPlayerOptions, float, bool, bool> callback)
        {
            if (Directory.Exists(settings.outputFolder))
            {
                foreach (var dir in Directory.GetDirectories(settings.outputFolder))
                {
                    if (!dir.EndsWith(".git"))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                foreach (var file in Directory.GetFiles(settings.outputFolder))
                {
                    File.Delete(file);
                }
            }

            var buildSteps = SelectedBuildOptions(settings);
            int i = 1;
            foreach (var opts in buildSteps)
            {
                if (callback != null &&
                    !callback(opts, (float)(i / (float)buildSteps.Count), false))
                {
                    return false; // cancelled
                }
                var report = BuildPipeline.BuildPlayer(opts);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("Build error. See log");
                }

                PostBuild(opts);

                ++i;
                if (callback != null &&
                    !callback(opts, (float)(i / (float)buildSteps.Count), true))
                {
                    return false; // cancelled
                }
            }

            string changelogPath = Path.Combine(Application.dataPath, "..", "changelog.txt");
            string changelogDestPath = Path.Combine(settings.outputFolder, "changelog.txt");
            if (File.Exists(changelogPath))
            {
                File.Copy(changelogPath, changelogDestPath, overwrite: true);
            }
            else
            {
                Debug.LogWarning("Could not copy changelog at path " + changelogPath);
            }

            return true;
        }

        public static void PostBuild(BuildPlayerOptions opts)
        {
            string buildDir = Path.GetDirectoryName(opts.locationPathName);
            if (Directory.Exists(buildDir))
            {
                string[] dirs = Directory.GetDirectories(buildDir, "*_BackUpThisFolder_ButDontShipItWithYourGame", SearchOption.TopDirectoryOnly);
                foreach (var dir in dirs)
                {
                    Directory.Delete(dir, true);
                }
            }
            else
            {
                Debug.LogWarning("Could not find build directory " + buildDir);
            }

            if (opts.target == BuildTarget.StandaloneWindows64)
            {
                string zipFile = Path.Combine(Path.GetDirectoryName(buildDir), "Idle_Factory_Win64.zip");
                ZipFile.CreateFromDirectory(buildDir, zipFile, System.IO.Compression.CompressionLevel.Optimal, false);
                Directory.Delete(buildDir, true);
            }
        }

        public static BuildTargetGroup GroupForTarget(BuildTarget t)
        {
            // Can't believe Unity doesn't have a method for this already
            switch (t)
            {
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;
#if UNITY_5_6_OR_NEWER
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
#endif
                // TODO more platforms?
                default:
                    return BuildTargetGroup.Unknown;
            }
        }

        static BuildTarget UnityTarget(TargetType t)
        {
            switch (t)
            {
                case TargetType.Win32:
                    return BuildTarget.StandaloneWindows;
                case TargetType.Win64:
                    return BuildTarget.StandaloneWindows64;
                case TargetType.Mac:
                case TargetType.Mac32:
                    return BuildTarget.StandaloneOSX;
                case TargetType.MacUniversal:
                    return BuildTarget.StandaloneOSX;
                case TargetType.Linux64:
                    return BuildTarget.StandaloneLinux64;
                case TargetType.iOS:
                    return BuildTarget.iOS;
                case TargetType.Android:
                    return BuildTarget.Android;
                case TargetType.WebGL:
                    return BuildTarget.WebGL;
                case TargetType.WinStore:
                    return BuildTarget.WSAPlayer;
                case TargetType.PS4:
                    return BuildTarget.PS4;
                case TargetType.XboxOne:
                    return BuildTarget.XboxOne;
                case TargetType.tvOS:
                    return BuildTarget.tvOS;
#if UNITY_5_6_OR_NEWER
                case TargetType.Switch:
                    return BuildTarget.Switch;
#endif
                // TODO more platforms?
                default:
                    throw new NotImplementedException("Target not supported");
            }
        }

        static public List<BuildPlayerOptions> SelectedBuildOptions(Settings settings)
        {
            var ret = new List<BuildPlayerOptions>();
            foreach (var target in settings.targets)
            {
                ret.Add(BuildOpts(settings, target));
            }
            return ret;
        }

        static public BuildPlayerOptions BuildOpts(Settings settings, Target target)
        {
            BuildPlayerOptions o = new BuildPlayerOptions();
            // Build all the scenes selected in build settings
            o.scenes = EditorBuildSettings.scenes
                .Where(x => x.enabled)
                .Select(x => x.path)
                .ToArray();
            string subfolder = target.ToString();
            o.locationPathName = target.outputFolder;
            o.target = UnityTarget(target.targetType);
            BuildOptions opts = BuildOptions.None;
            if (settings.developmentBuild)
                opts |= BuildOptions.Development;
            if (o.target == BuildTarget.Android)
                opts |= BuildOptions.AutoRunPlayer;
            o.options = opts;

            return o;
        }

        [PostProcessBuild(10)]
        public static void RemoveWebGLMobileStartupWarning(BuildTarget target, string targetPath)
        {
            var path = Path.Combine(targetPath, "Build/UnityLoader.js");
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                text = text.Replace("UnityLoader.SystemInfo.mobile", "false");
                File.WriteAllText(path, text);
            }
        }
    }
}
#endif