using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;


/*
 * This is for emulator developer not for emulator user. 
 */

/*
 * Research VRC component instance
 */
public class IwsdSpikeWindow : EditorWindow
{

    static IwsdSpikeWindow instance;

    [MenuItem("VRC_Iwsd/IwsdSpikeWindow")]
    static void OpenIwsdSpikeWindow()
    {
	if (instance != null) {
	    instance.Close();
	}

	instance = EditorWindow.CreateInstance<IwsdSpikeWindow>();
	instance.Show();
    }

    void OnInspectorUpdate()
    {
	Repaint();
    }

    string lastErrorString = null;
    int lastInstanceId;

    void OnGUI ()
    {
	EditorGUILayout.LabelField("IwsdSpikeWindow");
	EditorGUILayout.LabelField(" For research SerializedProperty");

	var active = Selection.activeGameObject;

	if (active == null)
        {
	    return;
	}
        
        if (active.GetInstanceID() != lastInstanceId)
        {
            lastErrorString = null;
            lastInstanceId = active.GetInstanceID();
        }
        
	EditorGUILayout.BeginHorizontal ();
	EditorGUILayout.LabelField("Selection.activeGameObject:");
        EditorGUILayout.LabelField(active.name);
        GUILayout.FlexibleSpace();
	EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();

	// var components = active.GetComponents<Component> ();
        foreach (VRCSDK2.VRC_Trigger triggerComp in active.GetComponents<VRCSDK2.VRC_Trigger>())
        {
            DrawUILine(Color.black, 1);

            SerializedObject triggerCompSerialized = new SerializedObject(triggerComp);

            // DrawTriggersReorderUI(triggerCompSerialized);
            DrawTriggers(triggerCompSerialized);
            // EditorGUILayout.Space();
        }

    }

    private void DrawTriggers(SerializedObject triggerCompSerialized)
    {
        EditorGUILayout.LabelField("VRC_Trigger.Triggers", new GUIStyle(){fontStyle = FontStyle.Bold});
        
        var triggersProp = triggerCompSerialized.FindProperty("Triggers");
        foreach (SerializedProperty triggerProperty in triggersProp)
        {
            SerializedProperty triggerTypeProperty = triggerProperty.FindPropertyRelative("TriggerType");
            SerializedProperty nameProperty = triggerProperty.FindPropertyRelative("Name");
            
            var typeValue = (VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue;
            var s = typeValue + ", name=" + nameProperty.stringValue;
            EditorGUILayout.LabelField(s);

            DrawTriggerEvents(triggerProperty);
        }
        
    }

    // internal name "event" means external name "action"
    private void DrawTriggerEvents(SerializedProperty triggerProperty)
    {
        SerializedProperty eventsList = triggerProperty.FindPropertyRelative("Events");
        foreach (SerializedProperty eventProperty in eventsList)
        {
            SerializedProperty name = eventProperty.FindPropertyRelative("Name");
            SerializedProperty evnetType = eventProperty.FindPropertyRelative("EventType");
            SerializedProperty parameterString = eventProperty.FindPropertyRelative("ParameterString");
            SerializedProperty parameterBytes = eventProperty.FindPropertyRelative("ParameterBytes");

            // propertyPath is like "Triggers.Array.data[0].Events.Array.data[0]"
            // EditorGUILayout.SelectableLabel(" event.propertyPath=" + eventProperty.propertyPath);

            var typeValue = (VRCSDK2.VRC_EventHandler.VrcEventType)evnetType.intValue;
            var s = "  t=" + typeValue + ", n=" + name.stringValue + ", s=" + parameterString.stringValue + ",";
            EditorGUILayout.LabelField(s);

            if (lastErrorString != null) {
                EditorGUILayout.LabelField(lastErrorString);
                return;
            }
            // VRC_Serialization is in Dependencies/VRChat/VRCSDK2.dll
#if UNITY_2018_4_OR_NEWER
            object[] parameters = VRC.SDKBase.VRC_Serialization.ParameterDecoder(VRCSDK2.VRC_EditorTools.ReadBytesFromProperty(parameterBytes));
#else
            object[] parameters = VRCSDK2.VRC_Serialization.ParameterDecoder(VRCSDK2.VRC_EditorTools.ReadBytesFromProperty(parameterBytes));
#endif
            if (parameters == null) {
                lastErrorString = "parameters == null";
                EditorGUILayout.LabelField(lastErrorString);
                return;
            }
            
            DrawParameterObjects(parameters);
        }
    }
    
    private void DrawParameterObjects(object[] parameters)
    {
        for (int idx = 0; idx < parameters.Length; ++idx)
        {
            var obj = parameters[idx];
            System.Type type = obj.GetType();
            var s = "    " + idx + ": type=" + type;
            EditorGUILayout.LabelField(s);

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                var unityObj = (UnityEngine.Object)obj;
                EditorGUILayout.ObjectField("      param" + idx, unityObj, type, true);
                EditorGUILayout.LabelField("      InstanceID=" + unityObj.GetInstanceID());
                
            }
        }
    }
                    
    
    ReorderableList triggerReorderableList;
    private void DrawTriggersReorderUI(SerializedObject triggerCompSerialized_)
    {
        var triggersProp = triggerCompSerialized_.FindProperty("Triggers").Copy();
        // var triggerReorderableList = new ReorderableList(triggerCompSerialized, triggersProp);
        triggerReorderableList = new ReorderableList(triggerCompSerialized_, triggersProp);
        triggerReorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField (rect, "Triggers order edit");
        triggerReorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
            var triggerProperty = triggersProp.GetArrayElementAtIndex (index);
            SerializedProperty triggerTypeProperty = triggerProperty.FindPropertyRelative("TriggerType");
            SerializedProperty nameProperty = triggerProperty.FindPropertyRelative("Name");
            
            var typeValue = (VRCSDK2.VRC_Trigger.TriggerType)triggerTypeProperty.intValue;
            var s = "" + index + ": " + typeValue + ", name=" + nameProperty.stringValue;
            EditorGUI.LabelField(rect, s);
        };
        
        triggerReorderableList.DoLayoutList();
    }
    // public override void OnInspectorGUI () {
    //     if (triggerCompSerialized != null) {
    //         triggerCompSerialized.Update();
    //         triggerReorderableList.DoLayoutList();
    //         triggerCompSerialized.ApplyModifiedProperties();
    //     }
    // }
    


    ////////////////////////////////////////////////////////////////////////////////

    private void HorizontalLayoutTest()
    {
        // what's the default? only 3?
        EditorGUILayout.BeginHorizontal();
	EditorGUILayout.LabelField("AAAAAAAAAwwww:");
        // EditorGUILayout.LabelField("<BBBBBBBBB>");
        // EditorGUILayout.LabelField("<BBBBBBBBB>", new GUIStyle(){alignment = TextAnchor.MiddleLeft});
	EditorGUILayout.LabelField(":CCCCCCC");
	EditorGUILayout.LabelField(":DDDD");
	EditorGUILayout.EndHorizontal ();
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    // https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
    // by alexanderameye
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
        r.height = thickness;
        r.y+=padding/2;
        r.x-=2;
        r.width +=6;
        EditorGUI.DrawRect(r, color);
    }

}


// Snipets

// EditorGUILayout.Space();
// EditorGUILayout.Toggle(bool);
// EditorGUILayout.LabelField(string);
// GUILayout.FlexibleSpace();
// new GUIStyle(){alignment = TextAnchor.MiddleLeft}
// Debug.Log( string );
// EditorGUILayout.SelectableLabel(string);
                                
// mScrollPos = EditorGUILayout.BeginScrollView( mScrollPos );
// EditorGUILayout.TextArea( mResult );
// EditorGUILayout.EndScrollView();

// var builder = new StringBuilder();
// builder.AppendLine( s );
// builder.ToString();


