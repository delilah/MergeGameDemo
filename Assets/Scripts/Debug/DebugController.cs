using System.Diagnostics;
using UnityEngine;

namespace MergeGame.MergeDebug
{
    public static class DebugController
    {
        public enum LogLevel { All, WarningsAndErrors, ErrorsOnly, None }

        public static bool EnableLogs = true;
        public static LogLevel Level = LogLevel.All;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            if (!EnableLogs || Level != LogLevel.All)
            {
                return;
            }

            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message)
        {
            if (!EnableLogs || Level > LogLevel.WarningsAndErrors)
            {
                return;
            }

            UnityEngine.Debug.LogWarning(message);
        }

        public static void LogError(string message)
        {
            if (!EnableLogs || Level > LogLevel.ErrorsOnly)
            {
                return;
            }
            
            UnityEngine.Debug.LogError(message);
        }
    }
}