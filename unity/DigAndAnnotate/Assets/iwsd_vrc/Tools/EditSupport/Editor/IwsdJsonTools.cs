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
            // Conver target object to JSON object
            //   (It can take any System.Object as target) 
            //   Conver to JSON string from 
            var cmpJsonStr = JsonUtility.ToJson(triggerComp, false);
            //   Convert to JSON object from JSON string 
            var cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);

            // Prepare export JSON object from template JSON string
            var expJsonTemplate = "{\"format\":\"" + jsonFormatName_partialObject + "\", objectType:null, \"hint\":{}, \"data\":{\"Triggers\":[]}}";
            var expJsonObj = SimpleJSON.JSON.Parse(expJsonTemplate);

            // Add meta info to export JSON object
            expJsonObj["hint"].Add("PlayerSettings.productName", PlayerSettings.productName);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            expJsonObj["hint"].Add("scene.name", scene.name);
            expJsonObj["hint"].Add("scene.path", scene.path);
            expJsonObj["hint"].Add("objectPath", IwsdSubInspectorWindow.GetGameObjectPath(triggerComp.gameObject));
            expJsonObj["hint"].Add("created", DateTime.UtcNow.ToString("o"));
            expJsonObj.Add("objectType", triggerComp.GetType().FullName); // specify type by full name

            // Export. Build export object as JSON object
            //   This case is partial object format. so
            //   Copy from target JSON object to export JSON object,
            var expTriggers = expJsonObj["data"]["Triggers"];
            var cmpTriggers = cmpJsonObj["Triggers"];
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

            // Convert to portable string (from export JSON object to JSON string)
            return expJsonObj.ToString();
        }

        private void ImportToTail(string importString, VRCSDK2.VRC_Trigger triggerComp)
        {
            // Parse importing JSON string to JSON object
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

            // Check format
            if ((expJsonObj["format"] != jsonFormatName_partialObject)
                || (expJsonObj["objectType"] != triggerComp.GetType().FullName))
            {
                OpResultType = MessageType.Error;
                OpResultMessage = "Unknown or unsupported format data in clipboard";
                return;
            }

            // Convert target object to JSON object
            //   Convert to JSON string
            var cmpJsonStr = JsonUtility.ToJson(triggerComp, false);
            //   Convert to JSON object
            var cmpJsonObj = SimpleJSON.JSON.Parse(cmpJsonStr);

            // Import.
            //   Extract importing JSON object info.
            //   Digest into target object.
            var cmpTriggers = cmpJsonObj["Triggers"];
            var expTriggers = expJsonObj["data"]["Triggers"];
            for (int i = 0; i < expTriggers.Count; i++)
            {
                cmpTriggers.Add(expTriggers[i]);
            }

            // Modify target object
            //   Convert modified target object to JSON string
            string modified = cmpJsonObj.ToString();
            //   Load/merge from JSON string
            Undo.RecordObject(triggerComp, "Paste to VRC_Trigger");
            JsonUtility.FromJsonOverwrite(modified, triggerComp);

            // Preprocess
            // (This could be done in while import step.)
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
}
