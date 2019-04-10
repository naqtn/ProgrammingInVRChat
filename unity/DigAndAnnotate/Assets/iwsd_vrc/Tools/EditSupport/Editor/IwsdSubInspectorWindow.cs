/*
 * Iwsd / Sub Inspector
 *
 * By naqtn (https://twitter.com/naqtn)
 * Hosted at https://github.com/naqtn/ProgrammingInVRChat
 * 
 */
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
    
namespace Iwsd
{

    public class IwsdSubInspectorWindow : EditorWindow
    {
        [MenuItem("Window/VRC_Iwsd/Open Sub Inspector")]
        static void OpenIwsdSubInspectorWindow()
        {
            EditorWindow.GetWindow<IwsdSubInspectorWindow>("Sub Inspector");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        string lastMessageString;
        MessageType lastMessageType = MessageType.None;
        int lastInstanceId;
        Vector2 scrollPosition = new Vector2(0, 0);
        
        void OnGUI ()
        {
            var active = Selection.activeGameObject;
            if (active == null)
            {
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Iwsd Sub Inspector", new GUIStyle(){fontStyle = FontStyle.Bold});
            
            // CHECK ExecuteInEditMode attribute and Update
            if ((active.GetInstanceID() != lastInstanceId) || CheckComponentAlignmentChanged(active))
            {
                lastInstanceId = active.GetInstanceID();
                lastMessageString = null;

                ReplaceEditors(active);
            }
        
            DrawGUI(active);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGUI(GameObject active)
        {
            if (lastMessageString != null)
            {
                EditorGUILayout.HelpBox(lastMessageString, lastMessageType);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Selected object path: (read only)");
            EditorGUILayout.TextField(GetGameObjectPath(active));
            // EditorGUILayout.SelectableLabel(GetGameObjectPath(active));
            EditorGUILayout.Space();

            DispatchOnGUI();
        }


        private static string GetGameObjectPath(GameObject anObject)
        {
            var buf = new System.Text.StringBuilder();
            GetGameObjectPathSub(anObject.transform, buf);
            return buf.ToString();
        }             

        private static void GetGameObjectPathSub(Transform transform, System.Text.StringBuilder buf)
        {
            if (transform.parent != null)
            {
                GetGameObjectPathSub(transform.parent, buf);
            }
            buf.Append('/');
            buf.Append(transform.name);
        }
        
        ////////////////////////////////////////////////////////////
        // "Inspector"
        
        // Component to Editor map
        // Key is full name of type string. It's not Type to support non public type (ex VRC_Panorama)
        static private Dictionary<string, Type> EditorRegistry;
    
        static IwsdSubInspectorWindow()
        {
            EditorRegistry = new Dictionary<string, Type>();

            // TODO gather implementation classes via custom attributes
            EditorRegistry.Add("UnityEngine.UI.Button", typeof(ButtonUnityEventOrderEditor));
            EditorRegistry.Add("UnityEngine.UI.InputField", typeof(InputFieldUnityEventOrderEditor));
            EditorRegistry.Add("VRCSDK2.VRC_Trigger", typeof(VRC_TriggerOrderEditor));
            // EditorRegistry.Add("VRCSDK2.scripts.Scenes.VRC_Panorama", typeof(VRC_PanoramaSimpleEditor));
            EditorRegistry.Add("VRCSDK2.scripts.Scenes.VRC_Panorama", typeof(VRC_PanoramaOrderEditor));
        }

        List<int> ComponentIds = new List<int>();
        List<Editor> EditorInstances = new List<Editor>();

        private bool CheckComponentAlignmentChanged(GameObject anObject)
        {
            int idx = 0;
            foreach (var compObj in anObject.GetComponents<Component>())
            {
                var compType = compObj.GetType();

                if (EditorRegistry.ContainsKey(compType.FullName))
                {
                    // for each supported component
                    if ((ComponentIds.Count <= idx ) || (ComponentIds[idx++] != compObj.GetInstanceID()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        // active object to inspect changed
        private void ReplaceEditors(GameObject anObject)
        {
            foreach (var editor in EditorInstances)
            {
                InvokeMethod(editor, "OnDisable");
            }
            EditorInstances.Clear();
            ComponentIds.Clear();
            
            foreach (var compObj in anObject.GetComponents<Component>())
            {
                var compTypeName = compObj.GetType().FullName;
                if (EditorRegistry.ContainsKey(compTypeName))
                {
                    InstantiateIwsdEditor(compTypeName, compObj);
                    ComponentIds.Add(compObj.GetInstanceID());
                }
            }
            
        }

        private void InstantiateIwsdEditor(string compTypeName, Component compObj)
        {
            Type editorType = EditorRegistry[compTypeName];

            // ScriptableObject instance = ScriptableObject.CreateInstance(editorType);
            // MEMO CreateInstance calls OnEnable on instantiation.
            // It's not too early for this inspector emulation.
            
            var ctor = editorType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[]{}, null);
            if (ctor == null)
            {
                Debug.LogError("Not found constructor. type=" + editorType);
                return;
            }
            
            // CHECK this implementation supports non public class ?
            object instance = ctor.Invoke(new object[]{});
            if (instance == null)
            {
                Debug.LogError("Faild to create instance. type=" + editorType);
                return;
            }
                
            Editor theEditor = instance as Editor;
            if (theEditor == null)
            {
                Debug.LogError("Invalid editor class registration. type=" + editorType + ", type of instance=" + instance.GetType());
                Debug.LogError("  expect subclass of type=" + typeof(Editor));
                Debug.LogError("  hum," + (instance is Editor));
                return;
            }

            theEditor.Initialize(compObj);
            InvokeMethod(theEditor, "OnEnable");
            EditorInstances.Add(theEditor);
        }

        
        private void DispatchOnGUI()
        {
            EditorGUILayout.BeginVertical(new GUIStyle(){margin = new RectOffset(){left=10,right=5,top=5}});
            foreach (var editor in EditorInstances)
            {
                editor.OnInspectorGUI();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }

        private object InvokeMethod(object anObject, string methodName)
        {
            Type type = anObject.GetType();
            MethodInfo minfo = type.GetMethod(methodName);
            if (minfo == null)
            {
                return null;
            }
            return minfo.Invoke(anObject, null);
        }

    }
        
    // Fake  UnityEditor.Editor : UnityEngine.ScriptableObject
    public class Editor
    {
        SerializedObject _serializedObject;
            
        public SerializedObject serializedObject
        {
            get
            {
                return _serializedObject;
            }
        }

        private string targetTypeName;
        private string headerString;
        
        // This is internal and not compatible with UnityEditor.Editor
        internal void Initialize(UnityEngine.Object targetComponent)
        {
            _serializedObject = new SerializedObject(targetComponent);
            targetTypeName = targetComponent.GetType().Name;
            headerString = targetTypeName + " (" + this.GetType() + ")";
        }

        public virtual void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(headerString, new GUIStyle(){fontStyle = FontStyle.Bold});
        }
    }



    
    ////////////////////////////////////////////////////////////
    // Editor implementations

    class ReorderEditor : Iwsd.Editor
    {
        protected ReorderableList reorderableList;
        protected string HeaderTitle;

        // subclass must implement OnEnable() to build reorderableList
            

        public void OnDisable()
        {
            reorderableList = null;
        }
            
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
            
    }
        
    // for VRCSDK2.VRC_Trigger
    class VRC_TriggerOrderEditor : ReorderEditor
    {
        public void OnEnable()
        {
            var triggersProp = serializedObject.FindProperty("Triggers");
            if (triggersProp == null)
            {
                Debug.LogError("triggersProp == null");
                return;
            }
                
            reorderableList = new ReorderableList(serializedObject, triggersProp);
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField (rect, "VRC_Trigger | Triggers");
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                // padding
                rect.height -= 4; 
                rect.y += 2;
                // VRCSDK2.VRC_Trigger+TriggerEvent
                var triggerProperty = triggersProp.GetArrayElementAtIndex(index);
                var s = SummarizeTriggerEvent(index, serializedObject, triggerProperty);

                EditorGUI.LabelField(rect, s);
            };
            // TODO add edit feature.
            reorderableList.onMouseUpCallback = (list) => {
                // Debug.Log("selected:" + list.index);
            };
        }


        private static string SummarizeTriggerEvent(int index, SerializedObject vrc_trigger, SerializedProperty triggerEvent)
        {
            SerializedProperty triggerTypeProperty = triggerEvent.FindPropertyRelative("TriggerType");
            var typeValue = (VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue;

            var buf = new System.Text.StringBuilder();

            buf.Append(index);
            buf.Append(": ");
            buf.Append(typeValue);
            switch (typeValue)
            {
                case VRCSDK2.VRC_Trigger.TriggerType.Custom:
                    var nameProperty = triggerEvent.FindPropertyRelative("Name");
                    buf.Append(" <Name=\"");
                    buf.Append(nameProperty.stringValue);
                    buf.Append("\">");
                    break;

                case VRCSDK2.VRC_Trigger.TriggerType.OnInteract:
                    var interactText = vrc_trigger.FindProperty("interactText");
                    buf.Append(" <interactText=\"");
                    buf.Append(interactText.stringValue);
                    buf.Append("\">");
                    break;

                case VRCSDK2.VRC_Trigger.TriggerType.OnKeyUp:
                case VRCSDK2.VRC_Trigger.TriggerType.OnKeyDown:
                    var key = triggerEvent.FindPropertyRelative("Key");
                    var keyInt = key.intValue;
                    buf.Append(" <Key=");
                    if (Enum.IsDefined(typeof(KeyCode), keyInt))
                    {
                        var keyCode = (KeyCode)Enum.ToObject(typeof(KeyCode), keyInt);
                        buf.Append(keyCode);
                    }
                    else
                    {
                        buf.Append("unknown_value_");
                        buf.Append(keyInt);
                    }
                    buf.Append(">");
                    break;

                case VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger:
                case VRCSDK2.VRC_Trigger.TriggerType.OnExitTrigger:
                case VRCSDK2.VRC_Trigger.TriggerType.OnEnterCollider:
                case VRCSDK2.VRC_Trigger.TriggerType.OnExitCollider:
                    var layers = triggerEvent.FindPropertyRelative("Layers");
                    var triggerIndividuals = triggerEvent.FindPropertyRelative("TriggerIndividuals");
                    buf.Append(" <Layers=\"");
                    buf.Append(LayerMaskNameStrings(layers.intValue));
                    buf.Append("\", individuals=");
                    buf.Append(FormatBooleanShort(triggerIndividuals.boolValue));
                    buf.Append(">");
                    break;

                case VRCSDK2.VRC_Trigger.TriggerType.OnTimer:
                    var repeat = triggerEvent.FindPropertyRelative("Repeat");
                    var lowPeriodTime = triggerEvent.FindPropertyRelative("LowPeriodTime");
                    var highPeriodTime = triggerEvent.FindPropertyRelative("HighPeriodTime");
                    var resetOnEnable = triggerEvent.FindPropertyRelative("ResetOnEnable");
                    buf.Append(" <Repeat=");
                    buf.Append(FormatBooleanShort(repeat.boolValue));
                    buf.Append(", ResetOnEnable=");
                    buf.Append(FormatBooleanShort(resetOnEnable.boolValue));
                    if (lowPeriodTime.floatValue == highPeriodTime.floatValue)
                    {
                        buf.Append(", time=");
                        buf.Append(lowPeriodTime.floatValue);
                    }
                    else
                    {
                        buf.Append(", low=");
                        buf.Append(lowPeriodTime.floatValue);
                        buf.Append(", high=");
                        buf.Append(highPeriodTime.floatValue);
                    }
                    buf.Append(">");
                    break;
            }

            return buf.ToString();
        }

        private static string FormatBooleanShort(bool b)
        {
            return b? "T": "F";
        }
        
        private static string LayerMaskNameStrings(int layerMask)
        {
            if (layerMask == -1)
            {
                return "Everything";
            }
            if (layerMask == 0)
            {
                return "Nothing";
            }

            var buf = new System.Text.StringBuilder();
            int v = 1;
            for (int i = 0; i < 32; i++)
            {
                if ((layerMask & 1) != 0)
                {
                    if (buf.Length != 0)
                    {
                        buf.Append('|');
                    }
                    buf.Append(LayerMask.LayerToName(i));
                }
                layerMask = layerMask >> 1;
                v = v << 1;
            }
            return buf.ToString();
        }
    }


    // for UnityEngine.UI.{Button, InputField}
    class UnityEventOrderEditor : ReorderEditor
    {
        // EventPropName : Property name of a event (that is inherited from UnityEvent)
        protected string EventPropName;

        public void OnEnable()
        {
            // TODO check type and not-null
            var m_OnClick = serializedObject.FindProperty(EventPropName);
            if (m_OnClick == null)
            {
                Debug.LogError("m_OnClick == null. EventPropName=" + EventPropName);
                return;
            }
            var m_PersistentCalls = m_OnClick.FindPropertyRelative("m_PersistentCalls");
            var m_Calls = m_PersistentCalls.FindPropertyRelative("m_Calls");
        
            reorderableList = new ReorderableList(serializedObject, m_Calls);
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField (rect, HeaderTitle);
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var persistentCall = m_Calls.GetArrayElementAtIndex(index);
                var m_MethodName = persistentCall.FindPropertyRelative("m_MethodName");
                var s = index + ": '" + m_MethodName.stringValue + "'";
                EditorGUI.LabelField(rect, s);
            };
        }

    }

    class ButtonUnityEventOrderEditor : UnityEventOrderEditor
    {
        public ButtonUnityEventOrderEditor()
        {
            EventPropName = "m_OnClick";
            HeaderTitle = "Button | On Click()";
        }
    }

    class InputFieldUnityEventOrderEditor : UnityEventOrderEditor
    {
        InputFieldUnityEventOrderEditor()
        {
            // EventPropName =  "m_OnEndEdit";
            // HeaderTitle =  "InputField | On End Edit(String)";
            EventPropName =  "m_OnValueChanged";
            HeaderTitle =  "InputField | On Value Changed(String)";
        }
    }
        
    // In case of Button
    // MonoBehaviour:   <= Button component
    //   ...
    //   m_OnClick:         <= inherited from UnityEvent
    //     m_PersistentCalls:
    //       m_Calls:           <= List<PersistentCall>
    //       - m_Target: {fileID: 878055904}
    //         m_MethodName: set_layer
    //         m_Mode: 3
    //         m_Arguments:
    //           m_ObjectArgument: {fileID: 0}
    //           m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
    //           m_IntArgument: 3
    //           m_FloatArgument: 0
    //           m_StringArgument: 
    //           m_BoolArgument: 0
    //         m_CallState: 2
    //       - m_Target: {fileID: 878055909}
    //         ...
    //     m_TypeName: UnityEngine.UI.Button+ButtonClickedEvent, UnityEngine.UI, Version=1.0.0.0,
    //       Culture=neutral, PublicKeyToken=null

    // In case of InputField
    //   m_OnEndEdit:
    //     m_PersistentCalls:
    //       m_Calls: []
    //     m_TypeName: UnityEngine.UI.InputField+SubmitEvent, UnityEngine.UI, Version=1.0.0.0,
    //       Culture=neutral, PublicKeyToken=null
    //   m_OnValueChanged:
    //     m_PersistentCalls:
    //       m_Calls: []
    //     m_TypeName: UnityEngine.UI.InputField+OnChangeEvent, UnityEngine.UI, Version=1.0.0.0,
    //       Culture=neutral, PublicKeyToken=null


    
    /**
     * Editor for VRCSDK2.scripts.Scenes.VRC_Panorama
     */
    class VRC_PanoramaSimpleEditor : Iwsd.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
             
            serializedObject.Update();
            
            EditorGUI.indentLevel++;
            panoramasGUI(serializedObject.FindProperty("panoramas"));
            EditorGUI.indentLevel--;
                
            serializedObject.ApplyModifiedProperties();
        }

        void panoramasGUI(SerializedProperty panoramas)
        {
            EditorGUILayout.LabelField("count of panoramas:" + panoramas.arraySize);

            for (int idx = 0; idx < panoramas.arraySize; idx++)
            {
                // VRCSDK2.scripts.Scenes.VRC_Panorama+PanoSpec
                var panoSpec = panoramas.GetArrayElementAtIndex(idx);

                // SerializedProperty of string
                var url = panoSpec.FindPropertyRelative("url");
                // SerializedProperty of PPtr<$Texture2D>
                var texture = panoSpec.FindPropertyRelative("texture");
                
                EditorGUILayout.LabelField(idx + ":");
                url.stringValue = EditorGUILayout.TextField("URL:", url.stringValue);
                EditorGUILayout.PropertyField(texture, new GUIContent("texture"));
            }
        }
        
    }


    /**
     * Yet another Editor for VRCSDK2.scripts.Scenes.VRC_Panorama
     */
    class VRC_PanoramaOrderEditor : Iwsd.Editor
    {

        ReorderableList reorderableList;
        int texDispSize = 40;
        int paddingSize = 3;
        
        public void OnDisable()
        {
            reorderableList = null;
        }
            
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var renderer = serializedObject.FindProperty("renderer");
            EditorGUILayout.PropertyField(renderer, new GUIContent("renderer"));

            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        public void OnEnable()
        {
            var panoramas = serializedObject.FindProperty("panoramas");
            reorderableList = new ReorderableList(serializedObject, panoramas);
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField (rect, "panoramas");
            reorderableList.elementHeight = paddingSize + EditorGUIUtility.singleLineHeight + texDispSize + paddingSize;
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                // VRCSDK2.scripts.Scenes.VRC_Panorama+PanoSpec
                var panoSpec = panoramas.GetArrayElementAtIndex(index);
                DrawPeoperty(rect, panoSpec, index);
            };
        }

        // PropertyDrawer.OnGUI like interface
        private void DrawPeoperty(Rect position, SerializedProperty panoSpec, int index)
        {
            var url = panoSpec.FindPropertyRelative("url");
            var texture = panoSpec.FindPropertyRelative("texture");

            // index + url
            var rect = new Rect (position) {
                y = position.y + paddingSize,
                height = EditorGUIUtility.singleLineHeight
            };
            url.stringValue = EditorGUI.TextField(rect, new GUIContent(index + " URL"), url.stringValue);

            // texture
            rect.y += paddingSize + rect.height;
            rect.height = texDispSize;
            texture.objectReferenceValue =
                EditorGUI.ObjectField(rect, new GUIContent("texture"), texture.objectReferenceValue, typeof(Texture2D), false);

            // (another simple way)
            // EditorGUI.PropertyField(rect, texture, new GUIContent("texture"));
        }
    }
    
}
