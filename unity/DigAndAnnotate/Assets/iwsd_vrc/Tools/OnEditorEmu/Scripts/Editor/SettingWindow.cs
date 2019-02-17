using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

using System;

using System.Reflection;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;


namespace Iwsd
{
    public class SettingWindow : EditorWindow
    {
        static SettingWindow instance;

        [MenuItem("Window/VRC_Iwsd/Emulator")]
        static void OpenSettingWindow()
        {
            EditorWindow.GetWindow<SettingWindow>("Emulator");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI ()
        {
            EditorGUILayout.LabelField("Emulator Setting", new GUIStyle(){fontStyle = FontStyle.Bold});

            EditorGUILayout.BeginHorizontal();

            LocalPlayerContext.EnableSimulator = EditorGUILayout.Toggle(" Enabled", LocalPlayerContext.EnableSimulator);
            if (GUILayout.Button("Set enable and start"))
            {
                LocalPlayerContext.EnableSimulator = true;
                EditorApplication.isPlaying = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Modify Project Settings", new GUIStyle(){fontStyle = FontStyle.Bold});
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Adjust Fixed Timesteps to 90fps"))
            {
                // Thanks for https://twitter.com/momoma_creative/status/1073535358040588296
                // default 0.02 = 1/50
                Time.fixedDeltaTime = 1.0f / 90;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            
        }
    }
}

