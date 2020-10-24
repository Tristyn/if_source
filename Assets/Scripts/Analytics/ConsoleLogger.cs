
using GameAnalyticsSDK;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class ConsoleLogger
{
    static ConcurrentDictionary<string, object> loggedStackTraces = new ConcurrentDictionary<string, object>();

    public static void PipeConsoleToGameAnalytics()
    {
        Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
    }

    public static void EndPipeConsoleToGameAnalytics()
    {
        Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
        loggedStackTraces.Clear();
    }

    static void LogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Log)
        {
            // Log the stacktrace only once per session
            // Stops the same exception being logged every update
            if (stackTrace.Length < 20 || loggedStackTraces.TryAdd(stackTrace, null))
            {
                Analytics.instance.NewErrorEvent(LogTypeToGAErrorSeverity(type), condition + Environment.NewLine + Environment.NewLine + stackTrace);
            }
        }
    }

    static GAErrorSeverity LogTypeToGAErrorSeverity(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                return GAErrorSeverity.Error;
            case LogType.Assert:
                return GAErrorSeverity.Error;
            case LogType.Warning:
                return GAErrorSeverity.Warning;
            case LogType.Log:
                return GAErrorSeverity.Info;
            case LogType.Exception:
                return GAErrorSeverity.Error;
            default:
                return GAErrorSeverity.Info;
        }
    }
}
