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
            var assets = Selection.assetGUIDs;
            for (int i = 0; i < assets.Length; i++)
            {
                //Debug.LogError("assets[" + i + "]='" + assets[i] + "'");
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);
                //Debug.LogError(" path='" + path + "'");
            }
            //  ref:2224:VRCSDK/Dependencies/VRChat/VRCSDK2.dll

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

            HideFlagsEdit(active);

            DispatchOnGUI();
        }

        // NOTE: https://forum.unity.com/threads/is-it-possible-to-fold-a-component-from-script-inspector-view.296333/
        // UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded

        // HideFlags
        // http://tsubakit1.hateblo.jp/entry/20140422/1398177224
        bool HideFlagsEditShow = false;
        private void HideFlagsEdit(GameObject active)
        {
            HideFlagsEditShow = EditorGUILayout.Foldout(HideFlagsEditShow, "Inspector Edit Protection", true);
            if (HideFlagsEditShow)
            {
                // Undo.RecordObject seems not to work for hideFlags.
                bool changed = false;

                EditorGUI.indentLevel++;
                var cur = IsNotEditable(active);
                var sel = EditorGUILayout.ToggleLeft("whole GameObject", cur);
                if (cur != sel)
                {
                    SetNotEditable(active, sel);
                    changed = true;
                }

                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(sel);
                foreach (Component comp in active.GetComponents<Component>())
                {
                    cur = IsNotEditable(comp);
                    sel = EditorGUILayout.ToggleLeft(comp.GetType().Name, cur);
                    if (cur != sel)
                    {
                        SetNotEditable(comp, sel);
                        changed = true;
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;

                if (changed)
                {
                    // Force Inspector redraw
                    // https://answers.unity.com/questions/333181/how-do-you-force-a-custom-inspector-to-redraw.html
                    EditorUtility.SetDirty(active);
                }

            }
        }

        private static bool IsNotEditable(UnityEngine.Object obj)
        {
            return (obj.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;
        }

        private static void SetNotEditable(UnityEngine.Object obj, bool b)
        {
            if (b)
            {
		obj.hideFlags |= HideFlags.NotEditable;
            }
            else
            {
		obj.hideFlags &= ~HideFlags.NotEditable;
            }
        }

        ////////////////////////////////////////////////////////////

        // REFINE move to utility class
        internal static string GetGameObjectPath(GameObject anObject)
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
        // Key is full name of type string. It's not `System.Type`. This design is for supporting non public type (ex VRC_Panorama)
        static private Dictionary<string, List<Type>> EditorRegistry;

        static IwsdSubInspectorWindow()
        {
            EditorRegistry = new Dictionary<string, List<Type>>();

            // TODO gather implementation classes via custom attributes
            RegisterEditor("UnityEngine.UI.Button", typeof(ButtonUnityEventOrderEditor));
            RegisterEditor("UnityEngine.UI.InputField", typeof(InputFieldOnValueChangedUnityEventOrderEditor));
            RegisterEditor("UnityEngine.UI.InputField", typeof(InputFieldOnEndEditUnityEventOrderEditor));
            RegisterEditor("VRCSDK2.VRC_Trigger", typeof(VRC_TriggerOrderEditor));
            RegisterEditor("VRCSDK2.VRC_Trigger", typeof(VRC_TriggerCopyPasteEditor));
            // RegisterEditor("VRCSDK2.scripts.Scenes.VRC_Panorama", typeof(VRC_PanoramaSimpleEditor)});
            RegisterEditor("VRCSDK2.scripts.Scenes.VRC_Panorama", typeof(VRC_PanoramaOrderEditor));

            RegisterEditor("UnityEngine.TextMesh", typeof(TextMeshTextEditor));
        }

        public static void RegisterEditor(string typeFullName, Type editorType)
        {
            if (!EditorRegistry.ContainsKey(typeFullName))
            {
                EditorRegistry.Add(typeFullName, new List<Type>());
            }

            EditorRegistry[typeFullName].Add(editorType);
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
                    InstantiateIwsdEditors(compTypeName, compObj);
                    ComponentIds.Add(compObj.GetInstanceID());
                }
            }

        }

        private void InstantiateIwsdEditors(string compTypeName, Component compObj)
        {
            List<Type> editorTypes = EditorRegistry[compTypeName];
            foreach (var t in editorTypes)
            {
                InstantiateIwsdEditor(t, compObj);
            }
        }

        private void InstantiateIwsdEditor(Type editorType, Component compObj)
        {
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


    /**
     * copy and paste support for VRCSDK2.VRC_Trigger's triggers through JSON text
     */
    class VRC_TriggerCopyPasteEditor : Iwsd.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUI.indentLevel++;
            triggerGUI(serializedObject.targetObject as VRCSDK2.VRC_Trigger);
            EditorGUI.indentLevel--;
        }

        List<bool> UserSelects = new List<bool>();
        string OpResultMessage = "";
        MessageType OpResultType = MessageType.None;


        void triggerGUI(VRCSDK2.VRC_Trigger triggerComp)
        {
            List<VRCSDK2.VRC_Trigger.TriggerEvent> triggers = triggerComp.Triggers;

            ensureUserSelectsSize(triggers);
            for (int i = 0; i < triggers.Count; i++)
            {
                UserSelects[i] = EditorGUILayout.ToggleLeft(LabelOf(i, triggers[i]), UserSelects[i]);
            }


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy to clipboard")) // "Copy to clipboard as JSON"
            {
                var result = ExportSelected(triggerComp);
                if (OpResultType == MessageType.Info) // means success (not good implementation design)
                {
                    GUIUtility.systemCopyBuffer = result;
                }
            }

            if (GUILayout.Button("Paste from clipboard"))
            {
                ImportToTail(GUIUtility.systemCopyBuffer, triggerComp);
            }
            EditorGUILayout.EndHorizontal();

            if (OpResultType != MessageType.None)
            {
                EditorGUILayout.HelpBox(OpResultMessage, OpResultType, true);
            }

        }

        private void ensureUserSelectsSize(List<VRCSDK2.VRC_Trigger.TriggerEvent> triggers)
        {
            if (UserSelects.Count != triggers.Count)
            {
                UserSelects.Clear();
                for (int i = 0; i < triggers.Count; i++)
                {
                    UserSelects.Add(false);
                }
            }
        }

        private string LabelOf(int idx, VRCSDK2.VRC_Trigger.TriggerEvent t)
        {
            var label = idx + ": " + t.TriggerType;
            if (t.TriggerType == VRCSDK2.VRC_Trigger.TriggerType.Custom)
            {
                label += "(" + t.Name + ")";
            }
            return label;
        }

        const string jsonFormatName_partialObject = "iwsd_vrc/Tools/PartialObject/v1.0";

        private string ExportSelected(VRCSDK2.VRC_Trigger triggerComp)
        {
            var cmpJsonStr = JsonUtility.ToJson(triggerComp, false);
            var cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);
            var cmpTriggers = cmpJsonObj["Triggers"];

            var expJsonTemplate = "{\"format\":\"" + jsonFormatName_partialObject + "\", objectType:null, \"hint\":{}, \"data\":{\"Triggers\":[]}}";
            var expJsonObj = SimpleJSON.JSON.Parse(expJsonTemplate);
            var expTriggers = expJsonObj["data"]["Triggers"];

            expJsonObj["hint"].Add("PlayerSettings.productName", PlayerSettings.productName);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            expJsonObj["hint"].Add("scene.name", scene.name);
            expJsonObj["hint"].Add("scene.path", scene.path);
            expJsonObj["hint"].Add("objectPath", IwsdSubInspectorWindow.GetGameObjectPath(triggerComp.gameObject));
            expJsonObj["hint"].Add("created", DateTime.UtcNow.ToString("o"));
            expJsonObj.Add("objectType", triggerComp.GetType().FullName); // specify type by full name

            int count = 0;
            for (int i = 0; i < UserSelects.Count; i++)
            {
                if (UserSelects[i]) {
                    expTriggers.Add(cmpTriggers[i]);
                    count++;
                }
            }

            if (count == 0)
            {
                OpResultType = MessageType.Warning;
                OpResultMessage = "No entries selected to copy";
            }
            else
            {
                OpResultType = MessageType.Info;
                OpResultMessage = "Copied. " + count + " entries to clipboard";
            }

            return expJsonObj.ToString();
        }

        private void ImportToTail(string importString, VRCSDK2.VRC_Trigger triggerComp)
        {
            SimpleJSON.JSONNode expJsonObj;
            try
            {
                expJsonObj = SimpleJSON.JSON.Parse(importString);
            }
            catch (Exception e)
            {
                OpResultType = MessageType.Warning;
                OpResultMessage = "not appropriate data in clipboard";
                return;
            }

            if ((expJsonObj["format"] != jsonFormatName_partialObject)
                || (expJsonObj["objectType"] != triggerComp.GetType().FullName))
            {
                OpResultType = MessageType.Error;
                OpResultMessage = "Unknown or unsupported format data in clipboard";
                return;
            }

            var cmpJsonStr = JsonUtility.ToJson(triggerComp, false);
            var cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);
            var cmpTriggers = cmpJsonObj["Triggers"];

            var expTriggers = expJsonObj["data"]["Triggers"];
            for (int i = 0; i < expTriggers.Count; i++)
            {
                cmpTriggers.Add(expTriggers[i]);
            }

            string modified = cmpJsonObj.ToString();
            Undo.RecordObject(triggerComp, "Paste to VRC_Trigger");
            JsonUtility.FromJsonOverwrite(modified, triggerComp);

            bool advanced = false;
            for (int i = 0; i < triggerComp.Triggers.Count; i++)
            {
                switch (triggerComp.Triggers[i].BroadcastType)
                {
                    case VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysUnbuffered:
                    case VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerUnbuffered:
                    case VRCSDK2.VRC_EventHandler.VrcBroadcastType.AlwaysBufferOne:
                    case VRCSDK2.VRC_EventHandler.VrcBroadcastType.OwnerBufferOne:
                        break;
                    default:
                        advanced = true;
                        break;
                }
            }
            if (advanced)
            {
                triggerComp.UsesAdvancedOptions = true;
            }

            OpResultType = MessageType.Info;
            OpResultMessage = expTriggers.Count + " entrie(s) added successfully";
            return;
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
            HeaderTitle = "Button | On Click ()";
        }
    }

    class InputFieldOnValueChangedUnityEventOrderEditor : UnityEventOrderEditor
    {
        InputFieldOnValueChangedUnityEventOrderEditor()
        {
            EventPropName =  "m_OnValueChanged";
            HeaderTitle =  "InputField | On Value Changed (String)";
        }
    }

    class InputFieldOnEndEditUnityEventOrderEditor : UnityEventOrderEditor
    {
        InputFieldOnEndEditUnityEventOrderEditor()
        {
            EventPropName =  "m_OnEndEdit";
            HeaderTitle =  "InputField | On End Edit (String)";
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


    class TextMeshTextEditor : Iwsd.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUI.indentLevel++;

            var text = serializedObject.FindProperty("m_Text");
            text.stringValue = EditorGUILayout.TextArea(text.stringValue, new GUIStyle(GUI.skin.textArea){wordWrap = true});

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }


    class PropertyDump : Iwsd.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // serializedObject.Update();

            DumpProperties(serializedObject);
        }

        // https://forum.unity.com/threads/enumarting-serializedproperty-throws-an-exception.511310/
        // https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/
        // 
        // MEMO: Since Unity2018.3, property.objectReferenceValue is 0 for both Missing and None case.
        // [Unity2018.3でのMissingが検出できない](https://teratail.com/questions/167668)
        // Check child property "m_FileID", its intValue is 0 for None, otherwise Missing.
        void DumpProperties(SerializedObject obj)
        {
            var s = DumpProperties(obj, 50);
            
            EditorGUILayout.TextArea(s, new GUIStyle(GUI.skin.textArea){wordWrap = true});
        }
        
        string DumpProperties(SerializedObject obj, int limit)
        {
            SerializedProperty prop = obj.GetIterator();
            var buf = new System.Text.StringBuilder();
            int skipUntil = -1;
            
            // not well at end of child propeties
            // foreach (var item in property) {
            //     SerializedProperty prop = (SerializedProperty)item;

            while (prop.Next(true))
            {            
                if (--limit < 0)
                {
                    buf.Append("...(snip tail)...\n");
                    break;
                }
                
                if (prop.depth <= skipUntil)
                {
                    skipUntil = -1;
                }

                if (skipUntil < 0)
                {
                    PrintPropertyItself(prop, buf);

                    // if (prop.propertyType == SerializedPropertyType.String)
                    // {
                    //     skipUntil = prop.depth;
                    //     Indent(prop.depth, buf);
                    //     buf.Append("(content omitted)\n");
                    // }

                }
            }

            return buf.ToString();
        }

        void PrintPropertyItself(SerializedProperty prop, System.Text.StringBuilder buf)
        {
            Indent(prop.depth, buf);
            buf.Append(prop.propertyType);
            buf.Append(": '");
            buf.Append(prop.name);
            buf.Append("', '");
            buf.Append(prop.propertyPath);
            buf.Append("'");
                    
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    buf.Append(", value='");
                    buf.Append(prop.stringValue);
                    buf.Append("'");
                    break;
                            
                case SerializedPropertyType.Integer:
                    buf.Append(", value='");
                    buf.Append(prop.intValue);
                    buf.Append("'");
                    break;
                            
                default:
                    break;
            }
                    
            buf.Append("\n");
        }

        private void Indent(int count, System.Text.StringBuilder buf)
        {
            while (0 < count-- )
            {
                buf.Append("  ");
            }
        }

    }
}
