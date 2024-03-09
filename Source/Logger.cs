using System;
using UnityEngine;

namespace Decalco
{
    internal class Logger : MonoBehaviour
    {
        internal const string modName = "Decalc'o'mania";
        internal static readonly string modVersion = typeof(Loader).Assembly.GetName().Version.ToString();
        internal const string logPrefix = "[Decalc'o'mania]: ";

        internal static void Log(string message)
        {
            Debug.Log(logPrefix + message);
        }

        internal static void Warn(string message)
        {
            Debug.LogWarning(logPrefix + message);
        }

        internal static void Error(string message)
        {
            Debug.LogError(logPrefix + message);
        }
        internal static void Error(string message, Exception e)
        {
            Debug.LogError(logPrefix + message);
            Debug.LogException(e);
        }
    }
}
