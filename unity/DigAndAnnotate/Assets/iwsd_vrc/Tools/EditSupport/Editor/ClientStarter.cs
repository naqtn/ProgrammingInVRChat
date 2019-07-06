/**
 * MIT License
 *
 * Copyright (c) 2019 Naqtn
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

/**
 * VRChat Client Starter
 *
 *
 * This Unity extension starts VRChat client automatically after world publishing is completed.
 * You can go directly into published world.
 *
 * After installation, open window via Unity menu Window > VRC_Iwsd > Client Starter
 *
 * When "Start Start" option is enabled, this closes VRChat SDK "Manage World in Browser" dialog.
 * Disenable it if you need to use that dialog.
 *
 * Other features
 * - Start manually.
 * - Open manage page with Web browser
 *
 *
 * Written by naqtn (https://twitter.com/naqtn)
 * Hosted at https://github.com/naqtn/ProgrammingInVRChat
 * If you have defect reports or feature requests, please post to GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues)
 *
 *
 * TODO hide "not logged in" warning when IsLoggedInWithCredentials becomes true (need?)
 * TODO show URL as copyable string (need?)
 * TODO show world id as copyable string (need?)
 * TODO preserve nonce and instance number and reasonably refresh, to meet players by multiple invoke
 * TODO add advanced option section to hide options that is not used so often like above (copyable string, nonce...).
 * TODO Add non-VR selection feature (need directly starting client instead of Application.OpenURL())
 * TODO Add TextField for world id and manually open it (need?)
 */
namespace Iwsd
{

    public class ClientStarterWindow : EditorWindow
    {

        [MenuItem("Window/VRC_Iwsd/Client Starter")]
        static void OpenClientStarterWindow()
        {
            EditorWindow.GetWindow<ClientStarterWindow>("VRC Client");
        }

        void OnEnable()
        {
        }

        void OnGUI ()
        {
            var settings = ClientStarter.settings;

            //// Label
            EditorGUILayout.LabelField("VRChat Client Starter", new GUIStyle(){fontStyle = FontStyle.Bold});

            //// Settings
            EditorGUI.BeginChangeCheck();
            settings.startAfterPublished
                = EditorGUILayout.Toggle(new GUIContent("Auto Start", "Start client after publish completed"),
                                         settings.startAfterPublished);
            settings.worldAccessLevel
                = (ClientStarter.WorldAccessLevel) EditorGUILayout.EnumPopup("Access",
                                                                                    settings.worldAccessLevel);
            if (EditorGUI.EndChangeCheck()) {
                settings.Store();
            }

            //// Operation buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Operations");
            if (GUILayout.Button("Start Now"))
            {
                ClientStarter.TryToOpenLaunchURL(settings.worldAccessLevel);
            }
            if (GUILayout.Button("Open Manage Page"))
            {
                ClientStarter.TryToOpenManageURL();
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();


            //// Info
            var result = ClientStarter.lastResult;
            if (!result.IsSucceeded)
            {
                EditorGUILayout.HelpBox(result.Value, MessageType.Warning);
            }
        }



    }
    ////////////////////////////////////////////////////////////////////////////////

    [InitializeOnLoad]
    public class ClientStarter {
        
        ////////////////////////////////////////////////////////////
        // sub structures

        public enum WorldAccessLevel
        {
            Public,      // access "", omit tailing ~ section
            FriendsPlus, // access "hidden"
            Friends,     // access "friends"
            InvitePlus,  // access "private", at last "~canRequestInvite"
            Invite,      // access "private"
        }

        public class Settings
        {
            public bool startAfterPublished;
            public WorldAccessLevel worldAccessLevel;

            private const string startAfterPublished_key = "Iwsd.ClientStarter.startAfterPublished";
            private const string worldAccessLevel_key = "Iwsd.ClientStarter.worldAccessLevel";

            internal static Settings Load()
            {
                var o = new Settings();
                o.startAfterPublished = EditorPrefs.GetBool(startAfterPublished_key, true);
                o.worldAccessLevel = (WorldAccessLevel)EditorPrefs.GetInt(worldAccessLevel_key, (int)WorldAccessLevel.Friends);
                return o;
            }
            
            internal void Store()
            {
                EditorPrefs.SetBool(startAfterPublished_key, this.startAfterPublished);
                EditorPrefs.GetInt(worldAccessLevel_key, (int)this.worldAccessLevel);
            }
        }

        public class Result {
            public bool IsSucceeded;
            public string Value;
            public Result(bool isSucceeded, string value)
            {
                IsSucceeded = isSucceeded;
                Value = value;
            }
        }

        ////////////////////////////////////////////////////////////
        // Auto start handling
        
        static public Settings settings;
        static public Result lastResult;
        
        static ClientStarter()
        {
            lastResult = new Result(true, "");
            settings = Settings.Load();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += PublishPolling;
            // EditorApplication.modifierKeysChanged += PublishPolling; // For manual spike
        }

        // How many update call to next dialog polling
        private const int CHECK_CYCLE_ONUPDATE = 50;
        private const int CHECK_TRIAL_LIMIT = 20;

        static private int callCount = 0;
        static private int trialCount = CHECK_TRIAL_LIMIT + 1;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                callCount = 0;
                trialCount = 0;
            }
        }

        private static void PublishPolling()
        {
            // Avoid calling FindObjectsOfTypeAll so often, to reduce CPU load 
            if ((CHECK_TRIAL_LIMIT < trialCount) || (++callCount % CHECK_CYCLE_ONUPDATE != 0))
            {
                return;
            }

            callCount = 0;
            trialCount++;

            // This option check must be after count up considering or it opens immediately when dialog opend and option turns on
            if (!settings.startAfterPublished)
            {
                return;
            }
                                   
            // Currently (VRCSDK-2019.06.25.21.13_Public) ContentUploadedDialog :
            // * appears after publish
            // * is used for world publishing only
            // * is restricted to one instance
            // so it's fit my purpose.
            var completeDialog = Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.ContentUploadedDialog)) as EditorWindow[];
            if (completeDialog.Length != 0)
            {
                var r = TryToOpenLaunchURL(settings.worldAccessLevel);
                if (r.IsSucceeded)
                {
                    completeDialog[0].Close();
                }
            }

        }


        ////////////////////////////////////////////////////////////
        // Do start actions
        
        private static Result TryToOpen(Result url)
        {
            if (url.IsSucceeded)
            {
                var s = url.Value;
                Debug.Log("will OpenURL url='" + s + "'");
                Application.OpenURL(s);
            }
            else
            {
                Debug.LogWarning(url.Value);
            }

            return url;
        }

        public static Result TryToOpenManageURL()
        {
            lastResult = TryToOpen(ComposeManageURL());
            return lastResult;
        }

        public static Result TryToOpenLaunchURL(WorldAccessLevel accessLevel)
        {
            lastResult = TryToOpen(ComposeLaunchURL(accessLevel));
            return lastResult;
        }

        public static Result ExtractSceneBlueprintId()
        {
            var vrcPipelineManager = Resources.FindObjectsOfTypeAll(typeof(VRC.Core.VRCPipelineManager)) as VRC.Core.VRCPipelineManager[];
            foreach (var pm in vrcPipelineManager)
            {
                // PrefabUtility.GetPrefabType returns None on Unity 2017.4.15f1, PrefabInstance on Unity 2017.4.28f1
                // And GetPrefabType is obsolete in Unity 2018.3.x. Use PrefabUtility.IsPartOfPrefabAsset instead
                PrefabType ptype = PrefabUtility.GetPrefabType(pm);
                if ((ptype == PrefabType.PrefabInstance) || (ptype == PrefabType.None))
                {
                    var blueprintId = pm.blueprintId;
                    if (blueprintId == null || blueprintId == "")
                    {
                        return new Result(false, "Not publishd yet? (blueprintId is empty)");
                    }
                        
                    return new Result(true, blueprintId);
                }
            }
            return new Result(false, "VRC_SceneDescriptor is missing? (vrcPipelineManager.Length=" + vrcPipelineManager.Length + ")");
        }
        

        public static Result ComposeLaunchURL(WorldAccessLevel accessLevel)
        {
            var bid = ExtractSceneBlueprintId();
            if (!bid.IsSucceeded)
            {
                return bid;
            }
            var blueprintId = bid.Value;

            if (!VRC.Core.APIUser.IsLoggedInWithCredentials)
            {
                return new Result(false, "Not logged in. (Open 'VRChat SDK/Settings to check and try again' )");
            }

            var user = VRC.Core.APIUser.CurrentUser;
            if (user == null)
            {
                return new Result(false, "user == null");
            }
            var userid = user.id;
            if (userid == null)
            {
                return new Result(false, "user.id == null");
            }

            var nonce = Guid.NewGuid();
            var instno = new System.Random().Next(1000, 9000);

            var access = accessStringOf(accessLevel);

            // NOTE 'ref' should be other value. But API is not documented.
            //  "vrchat://launch?ref=vrchat.com&id={blueprintId}:{instno}~{access}({userid})~nonce({nonce}){option}";

            var url = "vrchat://launch?ref=vrchat.com&id=" + blueprintId + ":" + instno;
            if (accessLevel != WorldAccessLevel.Public)
            {
                url += "~" + access + "("+ userid + ")~nonce(" + nonce + ")";

                if (accessLevel == WorldAccessLevel.InvitePlus)
                {
                    url += "~canRequestInvite";
                }
            }

            return new Result(true, url);
        }

        public static Result ComposeManageURL()
        {
            var bid = ExtractSceneBlueprintId();
            if (bid.IsSucceeded)
            {
                // https://vrchat.com/home/world/{blueprintId}
                return new Result(true, "https://vrchat.com/home/world/" + bid.Value);
            }
            return bid;
        }


        private static string accessStringOf(WorldAccessLevel wal)
        {
            switch (wal)
            {
                case WorldAccessLevel.Public:      return "";
                case WorldAccessLevel.FriendsPlus: return "hidden";
                case WorldAccessLevel.Friends:     return "friends";
                case WorldAccessLevel.InvitePlus:  return "private";
                case WorldAccessLevel.Invite:      return "private";
                default:
                    throw new NotImplementedException("Not implemented for " + wal);
            }
        }
    }
}

#endif // UNITY_EDITOR
