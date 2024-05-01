using System;
using System.Reflection;
using UnityEngine;

namespace Decalco
{
    internal class Logger
    {
        internal static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private const string logPrefix = "[Decalc'o'mania]: ";

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
