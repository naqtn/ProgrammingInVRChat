using UnityEngine;

namespace Iwsd
{
    
    // TODO Add on/off switch (or compile time switch i.e. preprocesor if)
    // #define TRACE_LOG_ON  // < Quick implementation

    // TODO Add params (variable count trailing string arguments)
    
    class Iwlog {
        public static void Trace2(string s)
        {
            #if TRACE_LOG_ON
            UnityEngine.Debug.Log(s);
            #endif
        }

        public static void Trace2(Object context, string s)
        {
            #if TRACE_LOG_ON
            UnityEngine.Debug.Log(s, context);
            #endif
        }

        public static void Trace(string s)
        {
            #if TRACE_LOG_ON
            UnityEngine.Debug.Log(s);
            #endif
        }

        public static void Trace(Object context, string s)
        {
            #if TRACE_LOG_ON
            UnityEngine.Debug.Log(s, context);
            #endif
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


    class EditorEnvUtil
    {
        public static void ShowNotificationOnGameView(string str)
        {
            ShowNotificationOnGameView(new GUIContent(str));
        }
        
        public static void ShowNotificationOnGameView(GUIContent content)
        {
            #if UNITY_EDITOR                    
            var assembly = typeof(UnityEditor.EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            UnityEditor.EditorWindow.GetWindow(type).ShowNotification(content);
            #endif
        }
    }
}
