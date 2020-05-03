using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Iwsd
{

    public class YamlClassNamesExamWindow : EditorWindow
    {
        [MenuItem("Window/VRC_Iwsd/Exam YamlClassNames")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<YamlClassNamesExamWindow>("TEST");
        }

        Vector2 scrollPosition = new Vector2(0, 0);
        string textAreaString = "";

        void OnGUI ()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("just a TextArea", new GUIStyle(){fontStyle = FontStyle.Bold});

            EditorGUILayout.TextArea(textAreaString, new GUIStyle(GUI.skin.textArea){wordWrap = true});

            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("TEST", GUILayout.ExpandWidth(false))) {
                textAreaString = ExamUnityType();
            }
        }


        // http://hecres.hatenablog.com/entry/2018/03/17/152620
        //  (having typo)
        // https://forum.unity.com/threads/yaml-class-id-reference.501959/
        
        public static string ExamUnityType()
        {
            // namespace UnityEditor
            //   internal sealed class UnityType
            // public static ReadOnlyCollection<UnityType> GetTypes();
            // public string name { get; }
            // public int persistentTypeID { get; }
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));
            var unityTypeType = assembly.GetType("UnityEditor.UnityType");

            var pInfo_persistentTypeID = unityTypeType.GetProperty("persistentTypeID");
            var pInfo_name = unityTypeType.GetProperty("name");

            var types = (System.Collections.IEnumerable)
                unityTypeType.InvokeMember("GetTypes", BindingFlags.InvokeMethod, null, null, new object[] {});


            var typeInfos = new SortedList<int, UnityTypeInfoExam>();
            foreach (var unityTypeObj in types)
            {
                int pTypeID = (int)pInfo_persistentTypeID.GetValue(unityTypeObj, null);
                string name = (string)pInfo_name.GetValue(unityTypeObj, null);
                // s += "{0x" + pTypeID.ToString("X8") + ", \"" + name + "\"}\n";

                var utInfo = new UnityTypeInfoExam(pTypeID, name);
                typeInfos.Add(utInfo.persistentTypeID, utInfo);
            }

            var s = "";
            foreach (var utInfo in typeInfos.Values )
            {
                // if (utInfo.CsType == null)
                s += utInfo.format();
            }
            return s;
        }        
    }

    internal class UnityTypeInfoExam
    {
        public int persistentTypeID { get; }
        public string name { get; }

        public Type CsType;
        
        public bool IsComponent;
        
#pragma warning disable 618         // (CS0618 A class member was marked with the Obsolete attribute)
        static Type[] searchBase = {
            typeof(GameObject), typeof(EditorWindow), typeof(ParticleSystemRenderer),
            typeof(Rigidbody), typeof(Rigidbody2D), typeof(AudioListener),
            typeof(Animator), typeof(TextMesh), typeof(WheelCollider), typeof(WindZone),
            typeof(Cloth), typeof(Tree), typeof(Canvas), typeof(ParticleSystem),
            typeof(SpriteMask), typeof(UnityEngine.Tilemaps.Tilemap),
            typeof(UnityEngine.Video.VideoPlayer),
            typeof(TerrainCollider), typeof(UnityEngine.AI.OffMeshLink),
            typeof(UnityEngine.Rendering.SortingGroup), 
            typeof(UnityEngine.Playables.PlayableDirector),
            typeof(UnityEngine.XR.WSA.WorldAnchor),
            typeof(GridLayout)
            // typeof(UnityEditor.Build.Reporting.BuildReport) // since 2018.1?
        };
#pragma warning restore 618

        static string[] nsCandidates = {
            "UnityEngine.", " UnityEditor.", "UnityEngine.Audio.", "UnityEngine.Video.",  "UnityEngine.Tilemaps.",
            "UnityEngine.AI.", "UnityEditor.Animations.", "UnityEngine.Rendering.", "UnityEngine.Playables.",
            "UnityEditor.Build.Reporting.",
            "UnityEngine.XR.WSA."
        };
        
        public UnityTypeInfoExam(int pTypeID, string name)
        {
            this.persistentTypeID = pTypeID;
            this.name = name;

            IsComponent = false;
            searchType();
        }
        
        void searchType()
        {
            Type unityComponentType = typeof(UnityEngine.Component);
            
            foreach (var sbase in searchBase)
            {
                var assembly = Assembly.GetAssembly(sbase);
                foreach (var ns in nsCandidates)
                {
                    Type t = assembly.GetType(ns + name);
                    if (t != null)
                    {
                        IsComponent = unityComponentType.IsAssignableFrom(t);
                        
                        CsType = t;
                        return;
                    }
                }
            }
        }

        
        public string format()
        {
            return "("
                + persistentTypeID.ToString()
                + ", " + (IsComponent? "true": "false")
                + ", \"" + name + "\", "
                + ((CsType != null) ? ("t(" + CsType.ToString() + ")"): "null")
                + "),\n";
        }
    }
}

/*
string unityAsm = typeof(GameObject).Assembly.FullName;
Debug.Log("unityAsm=" + unityAsm);
Debug.Log("unityAsm2=" + typeof(UnityEngine.AudioListener).Assembly.FullName);
Type utype = Type.GetType("UnityEngine." + unityClassName + ", " + unityAsm);
// Type utype = Type.GetType("UnityEngine." + unityClassName);
Debug.Log("unityClassName type= " + utype);
if (utype == null) {
    Debug.LogError("unityClassName=" + unityClassName);
}

var assembly = Assembly.GetAssembly(typeof(GameObject));
var unityType = assembly.GetType("UnityEngine." + unityClassName);
Debug.Log("unityType=" + unityType);

*/


