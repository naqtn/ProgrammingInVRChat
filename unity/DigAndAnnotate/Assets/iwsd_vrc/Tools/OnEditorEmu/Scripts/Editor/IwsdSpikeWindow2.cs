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

/*
 * Research VRC component classes
 */
public class IwsdSpikeWindow2 : EditorWindow
{

    static IwsdSpikeWindow2 instance;

    [MenuItem("VRC_Iwsd/IwsdSpikeWindow2")]
    static void OpenIwsdSpikeWindow2()
    {
	if (instance != null) {
	    instance.Close();
	}

	instance = EditorWindow.CreateInstance<IwsdSpikeWindow2>();
	instance.Show();
    }

    void OnInspectorUpdate()
    {
	Repaint();
    }

    bool tested = false;
    string result = "<not eval yet>";
    string examTypeName = "";
    string examTypeResult = "";

    static Type foo = typeof(VRCSDK2.VRC_EventHandler.VrcEvent);
    
    void OnGUI ()
    {
	EditorGUILayout.LabelField("IwsdSpikeWindow2");

        
	var s = EditorGUILayout.TextField(examTypeName);
        if (s != examTypeName)
        {
            examTypeName = s;
            try
            {
                // Type.AssemblyQualifiedName
                // VRCSDK2.VRC_EventHandler+VrcEvent, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45
                
                examTypeResult = ExamType(TypeByName(s));
            }
            catch (Exception e)
            {
                examTypeResult = "s='" + s + "', Message='" + e.Message + "'";
            }
        }
        EditorGUILayout.TextArea(examTypeResult);
            
	// EditorGUILayout.LabelField("aaaa:");
        // if (!tested) {
        //     // result = ExamEnum(typeof(VRCSDK2.VRC_EventHandler.VrcBooleanOp));
        //     // result = ExamType(typeof(VRCSDK2.VRC_EventHandler.VrcEvent));
        //     // result = ExamType(typeof(Array));
        //     // result = BinaryFormatterTest();
        //     result = ExamType(typeof(VRCSDK2.VRC_Pickup));
        //     tested = true;
        // }
        // EditorGUILayout.TextArea(result);
	// EditorGUILayout.LabelField("bbb:");
    }

    // https://stackoverflow.com/questions/20008503/get-type-by-name/20008954#20008954
    public static Type TypeByName(string name)
    {
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
        buf.Append("FullName:" + type.FullName + "\n");
        buf.Append("AssemblyQualifiedName:" + type.AssemblyQualifiedName + "\n");
            
        buf.Append("Public Fields {\n");
        foreach (FieldInfo info in type.GetFields())
        {
            ExamCustomAttributes(info, buf);
            if (info.IsPublic) {
                buf.Append(" public " + info.FieldType.FullName + " " + info.Name + ";\n");
            }
        }
        buf.Append("}\n");
        
        for (var current = type; current != null; current = current.BaseType) {
            buf.Append("  BaseType:" + current.FullName + "\n");
        }
    
        buf.Append("Members {\n");
        foreach (MemberInfo info in type.GetMembers())
        {
            if (info.DeclaringType == type) { // only direct declared
                buf.Append(" " + info.MemberType + " Name=" + info.Name + "\n");
            }
        }
        buf.Append("}\n");


        if (type.IsEnum)
        {
            buf.Append(ExamEnum(type));
        }
        
        return buf.ToString();
    }

    static string ExamEnum(Type type)
    {
        string[] strs = Enum.GetNames(type);

        string result = "Enum {\n";
        foreach (var s in strs) {
            result += s + ",\n";
        }
        result += "}\n";

        return result;
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
    
    static private void DumpTypeNames(VRCSDK2.VRC_Trigger triggerComp)
    {
        Debug.Log("VRC_Trigger Triggers:" + triggerComp.Triggers.GetType());

        // Debug.Log("VRC_Trigger 2 " + triggerComp.Triggers.Count);
        if (0 < triggerComp.Triggers.Count) {
            var triggerEvent = triggerComp.Triggers[0];
            Debug.Log("VRC_Trigger TriggerType:" + triggerEvent.TriggerType.GetType());
            Debug.Log("VRC_Trigger BroadcastType:" + triggerEvent.BroadcastType.GetType());
            Debug.Log("VRC_Trigger VrcEvent:" + triggerEvent.Events.GetType());
            Debug.Log("VRC_Trigger Name:" + triggerEvent.Name.GetType());
            Debug.Log("VRC_Trigger oProbabilities:" + triggerEvent.Probabilities.GetType());
                

            if (0 < triggerEvent.Events.Count) {
                var vrcEvent = triggerEvent.Events[0];
                Debug.Log("VRC_Trigger VrcEvent Name:" + vrcEvent.Name.GetType());
                Debug.Log("VRC_Trigger VrcEvent EventType:" + vrcEvent.EventType.GetType());
                Debug.Log("VRC_Trigger VrcEvent ParameterString:" + vrcEvent.ParameterString.GetType());
                Debug.Log("VRC_Trigger VrcEvent ParameterBoolOp:" + vrcEvent.ParameterBoolOp.GetType());
                Debug.Log("VRC_Trigger VrcEvent ParameterFloat: " + vrcEvent.ParameterFloat.GetType());
                Debug.Log("VRC_Trigger VrcEvent ParameterInt: " + vrcEvent.ParameterInt.GetType());
                Debug.Log("VRC_Trigger VrcEvent ParameterObjects: " + vrcEvent.ParameterObjects.GetType());
            }
        }
    }


    static string BinaryFormatterTest()
    {
        // UnityEngine.Vector4 is not marked as Serializable
        // We need serialize surrogate.
        
        var formatter = new BinaryFormatter();

        var selector  = new SurrogateSelector();
        var context   = new StreamingContext(StreamingContextStates.All);
        // This code does not work now. Vector4_SerializationSurrogate is moved to SerializeUtil.cs
        // selector.AddSurrogate(typeof(UnityEngine.Vector4), context, new Iwsd.Vector4_SerializationSurrogate());
        formatter.SurrogateSelector = selector;

        var mem = new MemoryStream();
        var obj = new object[1]{new Vector4(1,0,0,0)}; // 
        formatter.Serialize(mem, obj);
        mem.Position = 0;

        var buf = new System.Text.StringBuilder();
        int b;
        while (0 <= (b = mem.ReadByte())) {
            buf.Append(b.ToString("x2"));
        }
        
        return buf.ToString();
    }
    
}



// VRCSDK2.VRC_EventHandler.VrcEvent
    
// Type type = Type.GetType("VRCSDK2.VRC_Trigger+TriggerType");
// string type = typeof(VRCSDK2.VRC_Trigger.TriggerType).FullName;
// string[] itemKindTypeNames = Enum.GetNames(typeof(VRCSDK2.VRC_EventHandler.VrcBooleanOp));

