using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

using System;

using System.Reflection;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;


/*
 * This is for emulator developer not for emulator user. 
 */


namespace Iwsd
{

    /*
     * ComponentBrowser
     * for Research VRChat component classes
     */
    public class ComponentBrowser : EditorWindow
    {
        [MenuItem("Window/VRC_Iwsd/ComponentBrowser")]
        static void OpenComponentBrowser()
        {
            EditorWindow.GetWindow<ComponentBrowser>("Comp Browser");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        Vector2 scrollPosition = new Vector2(0, 0);

        bool inputAsText = false;
        string inputTypeStr = "";
        Type inputType;
        
        void OnGUI ()
        {
            EditorGUILayout.LabelField("ComponentBrowser", new GUIStyle(){fontStyle = FontStyle.Bold});

            var active = Selection.activeGameObject;
            UpdateSelectItemsIfNeeded(active);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            inputAsText = EditorGUILayout.ToggleLeft("input Type as text:", inputAsText, GUILayout.ExpandWidth(false));
            GUI.enabled = inputAsText;
            var s = EditorGUILayout.TextField(inputTypeStr);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !GUI.enabled;
            compSelection = GUILayout.SelectionGrid(compSelection, selectContents, 5, //
                                                    new GUIStyle(GUI.skin.button){alignment = TextAnchor.MiddleLeft}, //
                                                    GUILayout.ExpandWidth(false));
            GUI.enabled = true;

            Type type = null;
            if (inputAsText)
            {
                if (s != inputTypeStr)
                {
                    inputTypeStr = s;
                    inputType = TypeByName(s);
                }
                type = inputType;
            }
            else
            {
                if (compSelection < selectTypes.Length) // if activeGameObject == null then Length = 0, compSelection = 0
                {
                    type = selectTypes[compSelection];
                }
            }
            
            drawType(type);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!(compSelection < selectTypes.Length)))
            {
                if (GUILayout.Button("Copy as JSON", GUILayout.ExpandWidth(false))) {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(active.GetComponent(selectTypes[compSelection]), true);
                }
                if (GUILayout.Button("Copy with EditorJsonUtility", GUILayout.ExpandWidth(false))) {
                    GUIUtility.systemCopyBuffer = EditorJsonUtility.ToJson(active.GetComponent(selectTypes[compSelection]), true);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private Type examingType;
        private string examTypeResult = "";
        void drawType(Type type)
        {
            if (type != examingType)
            {
                examingType = type;
                try
                {
                    examTypeResult = ExamType(examingType);
                }
                catch (Exception e)
                {
                    examTypeResult = "type='" + examingType + "', Message='" + e.Message + "'";
                }


                // Move focus because TextArea dosn't change its appearance if having focus. (... I don't know this is proper way.)
                GUI.SetNextControlName("dummy");
                GUILayout.Label("");
                GUI.FocusControl("dummy");

            }
            
            EditorGUILayout.TextArea(examTypeResult);
        }            

        private GUIContent[] selectContents;
        private Type[] selectTypes;
        private int compSelection;

        private void UpdateSelectItemsIfNeeded(GameObject anObject)
        {
            Component[] comps = (anObject == null)? new Component[0]: anObject.GetComponents<Component>();
            if (Equals(comps, selectTypes))
            {
                return;
            }
            
            selectContents = new GUIContent[comps.Length];
            selectTypes = new Type[comps.Length];
            compSelection = 0;
    
            for (int i = 0; i < comps.Length; i++)
            {
                Type type = comps[i].GetType();
                selectTypes[i] = type;
                selectContents[i] = new GUIContent(type.Name, type.FullName);
            }
        }

        private static bool Equals(Component[] comps, Type[] types)
        {
            if ((comps == null) || (types == null) || (comps.Length != types.Length))
            {
                return false;
            }
            
            for (int i = 0; i < comps.Length; i++)
            {
                var t = comps[i].GetType();
                if (!t.Equals(types[i]))
                {
                    return false;
                }
            }
            return true;
        }

        
            
        // https://stackoverflow.com/questions/20008503/get-type-by-name/20008954#20008954
        public static Type TypeByName(string name)
        {
            if (name == "")
            {
                return null;
            }
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) // .Reverse()
            {
                var tt = assembly.GetType(name);
                if (tt != null)
                {
                    return tt;
                }
            }
        
            return null;
        }

    
        static string ExamType(Type type)
        {
            if (type == null)
            {
                return "type is null";
            }
        
            var buf = new System.Text.StringBuilder();
            
            buf.Append("Name:" + type.Name + "\n");
            buf.Append("\n");
            buf.Append("FullName:" + type.FullName + "\n");
            buf.Append("AssemblyQualifiedName:" + type.AssemblyQualifiedName + "\n");
            buf.Append("Attributes:" + type.Attributes + "\n");
            buf.Append("Visibility:" + (type.Attributes & TypeAttributes.VisibilityMask) + "\n");

            
            // Properties
            buf.Append("\n");
            buf.Append("uGUI writable Properties (without wellknown) {\n");
            foreach (PropertyInfo info in type.GetProperties())
            {
                if (!IsWellKnown(info.DeclaringType)
                    && Is_uGUICallable(info))
                {
                    buf.Append("  ");
                    buf.Append(info.Name);
                    buf.Append(" , PropertyType='");
                    buf.Append(info.PropertyType.FullName);
                    buf.Append("'");
                    if (info.CanRead)
                    {
                        buf.Append(", Read");
                    }
                    if (info.CanWrite)
                    {
                        buf.Append(", Write");
                    }
                    if (info.IsSpecialName)
                    {
                        buf.Append(", SpecialName");
                    }
                    ExamCustomAttributes(info, buf);
                    buf.Append("\n");
                }
            }
            buf.Append("}\n");


            // Method
            buf.Append("\n");
            buf.Append("uGUI callable Methods  (without wellknown) {\n");

            // https://stackoverflow.com/questions/5030537/gettype-getmethods-returns-no-methods-when-using-a-bindingflag
            var mflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (MethodInfo info in type.GetMethods(mflags))
            {
                if (!IsWellKnown(info.DeclaringType)
                    && Is_uGUICallable(info))
                {
                    ParameterInfo[] parameters = info.GetParameters();

                    buf.Append("  ");
                    buf.Append(info.Name);
                    if (!info.IsPublic)
                    {
                        buf.Append(info.IsPrivate? " <private>": " <internal>");
                    }
                    if (1 <= parameters.Length)
                    {
                        buf.Append(" , Parameter0='");
                        buf.Append(parameters[0].ParameterType.FullName);
                        buf.Append("'");
                    }
                    ExamCustomAttributes(info, buf);
                    buf.Append("\n");
                }
            }
            buf.Append("}\n");


    
            // All Fields
            buf.Append("\n");
            buf.Append("All fields {\n");
            foreach (FieldInfo info in type.GetFields())
            {
                // buf.Append(" public " + info.FieldType.FullName + " " + info.Name + ";\n");
                buf.Append("  '");
                buf.Append(info.Name);
                buf.Append("'");
                ExamCustomAttributes(info, buf);
                if (info.IsPublic) {
                    buf.Append(", public ");
                }
                if (info.IsStatic)
                {
                    buf.Append(",static ");
                }
                buf.Append(", FieldType='");
                buf.Append(info.FieldType.FullName);
                buf.Append("'\n");
            }
            buf.Append("}\n");

            // Members
            buf.Append("\n");
            buf.Append("declared Members {\n");
            foreach (MemberInfo info in type.GetMembers())
            {
                if (info.DeclaringType == type) { // only direct declared
                    buf.Append("  ");
                    buf.Append(info.MemberType);
                    buf.Append("  Name='");
                    buf.Append(info.Name);
                    buf.Append("'\n");
                }
            }
            buf.Append("}\n");

            // Enum names
            if (type.IsEnum)
            {
                ExamEnum(type, buf);
            }

            // Inheritance (BaseType)
            buf.Append("\n");
            buf.Append("Inheritance {\n");
            for (var current = type; current != null; current = current.BaseType) {
                buf.Append("  '");
                buf.Append(current.FullName);
                buf.Append("'\n");
            }
            buf.Append("}\n");
    
            return buf.ToString();
        }

    
        static void ExamEnum(Type type, System.Text.StringBuilder buf)
        {
            string[] strs = Enum.GetNames(type);

            buf.Append("Enum {\n");
            foreach (var s in strs) {
                buf.Append("  '");
                buf.Append(s);
                buf.Append("'\n");
            }
            buf.Append("}\n");
        }


        static void ExamCustomAttributes(MemberInfo info, System.Text.StringBuilder buf)
        {
            Attribute[] attributes = Attribute.GetCustomAttributes(info);
            foreach (Attribute att in attributes)
            {
                buf.Append("[");
                buf.Append(att.GetType().FullName);
                buf.Append("]");
            }
        }

        static bool Is_uGUICallable(PropertyInfo info)
        {
            if (!info.CanWrite)
            {
                return false;
            }
            var ptype = info.PropertyType;
            if (!(ptype.Equals(typeof(float))
                  || ptype.Equals(typeof(int))
                  || ptype.Equals(typeof(string))
                  || typeof(UnityEngine.Object).IsAssignableFrom(ptype)))
            {
                return false;
            }
            return true;
        }
        
        static bool Is_uGUICallable(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            if (!info.ReturnType.Equals(typeof(void))
                // || !info.IsPublic  // Actually, private could be called!
                || info.IsSpecialName
                || info.IsStatic
                || info.IsGenericMethod
                || (1 < parameters.Length))
            {
                return false;
            }

            if (1 <= parameters.Length)
            {
                var ptype = parameters[0].ParameterType;
                if (!(ptype.Equals(typeof(float))
                      || ptype.Equals(typeof(int))
                      || ptype.Equals(typeof(string))
                      || typeof(UnityEngine.Object).IsAssignableFrom(ptype)))
                {
                    return false;
                }
            }
            return true;
        }

        
        static bool IsWellKnown(Type t)
        {
            return t.IsAssignableFrom(typeof(UnityEngine.MonoBehaviour));
        }
        

    }
}
