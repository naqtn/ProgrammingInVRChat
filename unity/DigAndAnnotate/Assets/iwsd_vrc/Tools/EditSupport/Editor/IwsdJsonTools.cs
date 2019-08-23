/*
 * Iwsd / JSON tools
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
            JsonOperationResult r = JsonTools.ExportPartial_VRC_Trigger_Trigger(triggerComp, UserSelects);
            OpResultType = r.Type;
            OpResultMessage = r.Message;
            return r.Output;
        }

        private void ImportToTail(string importString, VRCSDK2.VRC_Trigger triggerComp)
        {
            JsonOperationResult r = JsonTools.ImportPartial_VRC_Trigger_Trigger(importString, triggerComp);
            OpResultType = r.Type;
            OpResultMessage = r.Message;
        }

    }


    public class JsonOperationResult
    {
        // REFINE define own enum?
        public MessageType Type;

        public string Message;

        public string Output;

        internal JsonOperationResult(MessageType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }
    }

    public class JsonTools
    {
        JsonTools()
        {
        }

        public delegate JsonOperationResult JsonModifier(SimpleJSON.JSONNode input, SimpleJSON.JSONNode output);


        const string jsonFormatName_partialObject = "iwsd_vrc/Tools/PartialObject/v1.0";

        static public JsonOperationResult ExportPartial(UnityEngine.Component targetComp, JsonModifier jsonModifier)
        {
            // Conver target object to JSON object
            //   (It can take any System.Object as target)
            //   Conver to JSON string from
            string cmpJsonStr = JsonUtility.ToJson(targetComp, false);
            //   Convert to JSON object from JSON string
            SimpleJSON.JSONNode cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);

            // Prepare export JSON object from template JSON string
            string expJsonTemplate = "{\"format\":\"" + jsonFormatName_partialObject + "\", objectType:null, \"hint\":{}, \"data\":{}";
            SimpleJSON.JSONNode expJsonObj = SimpleJSON.JSON.Parse(expJsonTemplate);

            // Add meta info to export JSON object
            expJsonObj["hint"].Add("PlayerSettings.productName", PlayerSettings.productName);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            expJsonObj["hint"].Add("scene.name", scene.name);
            expJsonObj["hint"].Add("scene.path", scene.path);
            expJsonObj["hint"].Add("objectPath", IwsdSubInspectorWindow.GetGameObjectPath(targetComp.gameObject));
            expJsonObj["hint"].Add("created", DateTime.UtcNow.ToString("o"));
            expJsonObj.Add("objectType", targetComp.GetType().FullName); // specify type by full name

            // Copy partial elements from target JSON object to export JSON object
            JsonOperationResult r = jsonModifier(cmpJsonObj, expJsonObj);

            // Convert to portable string (from export JSON object to JSON string)
            r.Output = expJsonObj.ToString();

            return r;

            // TODO use EditorJsonUtility
        }


        static public JsonOperationResult ImportPartial(string importString, UnityEngine.Component targetComp, JsonModifier jsonModifier)
        {
            // Parse importing JSON string to JSON object
            SimpleJSON.JSONNode expJsonObj;
            try
            {
                expJsonObj = SimpleJSON.JSON.Parse(importString);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new JsonOperationResult(MessageType.Warning, "not appropriate data");
            }

            // Check format
            if ((expJsonObj["format"] != jsonFormatName_partialObject)
                || (expJsonObj["objectType"] != targetComp.GetType().FullName))
            {
                return new JsonOperationResult(MessageType.Error, "Unknown or unsupported format data");
            }

            // Convert target object to JSON object
            //   Convert to JSON string
            var cmpJsonStr = JsonUtility.ToJson(targetComp, false);
            //   Convert to JSON object
            var cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);

            JsonOperationResult r = jsonModifier(cmpJsonObj, expJsonObj);

            // Modify target object
            //   Convert modified target object to JSON string
            string modified = cmpJsonObj.ToString();
            //   Load/merge from JSON string
            Undo.RecordObject(targetComp, "Paste to VRC_Trigger");
            JsonUtility.FromJsonOverwrite(modified, targetComp);

            return r;

        }


        ////////////////////////////////////////

        static public JsonOperationResult ExportPartial_VRC_Trigger_Trigger(UnityEngine.Component targetComp, List<bool> selection)
        {

            return ExportPartial(targetComp, (SimpleJSON.JSONNode cmpJsonObj, SimpleJSON.JSONNode expJsonObj) => {
                    var cmpTriggers = cmpJsonObj["Triggers"];
                    var expTriggers = expJsonObj["data"]["Triggers"].AsArray;

                    int count = 0;
                    for (int i = 0; i < selection.Count; i++)
                    {
                        if (selection[i]) {
                            expTriggers.Add(cmpTriggers[i]);
                            count++;
                        }
                    }

                    return
                    (count == 0)?
                    new JsonOperationResult(MessageType.Warning, "No entries selected to copy")
                    : new JsonOperationResult(MessageType.Info, "Copied. (" + count + " entries)");
                });

        }

        static public JsonOperationResult ImportPartial_VRC_Trigger_Trigger(string importString, VRCSDK2.VRC_Trigger triggerComp)
        {
            return ImportPartial(importString, triggerComp, (SimpleJSON.JSONNode cmpJsonObj, SimpleJSON.JSONNode expJsonObj) => {
                    // Import.
                    //   Extract importing JSON object info.
                    //   Digest into target object.
                    var cmpTriggers = cmpJsonObj["Triggers"];
                    var expTriggers = expJsonObj["data"]["Triggers"];
                    for (int i = 0; i < expTriggers.Count; i++)
                    {
                        cmpTriggers.Add(expTriggers[i]);
                    }


                    bool advanced = false;
                    foreach (var c in cmpTriggers.Values)
                    {
                        switch ((VRCSDK2.VRC_EventHandler.VrcBroadcastType)c["BroadcastType"].AsLong)
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
                        cmpJsonObj["UsesAdvancedOptions"] = true;
                    }

                    return new JsonOperationResult(MessageType.Info,
                                                   expTriggers.Count + " entrie(s) added successfully");
                });
         }
     }
}
