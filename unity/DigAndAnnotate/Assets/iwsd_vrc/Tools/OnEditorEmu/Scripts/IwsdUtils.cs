using UnityEngine;

namespace Iwsd
{
    
    // TODO Add on/off switch (or compile time ifdef)
    // TODO Add params (variable count trailing string arguments)
    
    class Iwlog {
        public static void Trace2(string s)
        {
            // UnityEngine.Debug.Log(s);
        }

        public static void Trace(string s)
        {
            // UnityEngine.Debug.Log(s);
        }

        public static void Trace(Object context, string s)
        {
            // UnityEngine.Debug.Log(s, context);
        }

        public static void Debug(string s)
        {
            UnityEngine.Debug.Log(s);
        }

        public static void Debug(Object context, string s)
        {
            UnityEngine.Debug.Log(s, context);
        }

        public static void Warn(string s)
        {
            UnityEngine.Debug.LogWarning(s);
        }

        public static void Warn(Object context, string s)
        {
            UnityEngine.Debug.LogWarning(s, context);
        }

        public static void Error(string s)
        {
            UnityEngine.Debug.LogError(s);
        }

        public static void Error(Object context, string s)
        {
            UnityEngine.Debug.LogError(s, context);
        }
    }

    class StringUtil {
        public static string LastPartOf(string s, System.Char delm)
        {
            var i = s.LastIndexOf(delm);
            return s.Substring(i + 1);
        }
    }
}
