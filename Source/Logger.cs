using System;
using UnityEngine;

namespace Decalco
{
    class Logger : MonoBehaviour
    {
        public static string modName = "Decalc'o'mania";
        public static readonly string logPrefix = "[Decalc'o'mania]";

        public static void Log(string message)
        {
            Debug.Log($"{logPrefix} {message}");
        }

        public static void Warn(string message)
        {
            Debug.LogWarning($"{logPrefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{logPrefix} {message}");
        }
        public static void Error(string message, Exception e)
        {
            Debug.LogError($"{logPrefix} {message}");
            Debug.LogException(e);
        }
    }
}
