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
using System.Text.RegularExpressions;

/**
 * VRChat Client Starter
 *  ver. ClientStarter-20190718
 *
 * This Unity editor extension starts VRChat client automatically after world publishing is completed.
 * You can go directly into published world and start to test it.
 *
 * After installation, open window via Unity menu Window > VRC_Iwsd > Client Starter
 *
 * When "Start After Publish" option is enabled, this closes VRChat SDK "Manage World in Browser" dialog.
 * Disenable it if you need to use that dialog.
 *
 * Other features
 * - Start client manually
 * - Open manage page at vrchat.com
 * - "Start After Publish" feature works even if setting window is not opened
 * - and more
 *
 * Written by naqtn (https://twitter.com/naqtn)
 * Hosted at https://github.com/naqtn/ProgrammingInVRChat
 * If you have defect reports or feature requests, please post to GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues)
 *
 */
namespace Iwsd
{
    // IDEA hide "not logged in" warning when IsLoggedInWithCredentials becomes true (need? Polling is not good)
    // IDEA preserve nonce and instance number and reasonably refresh.  (need?)

    public class ClientStarterWindow : EditorWindow
    {

        [MenuItem("Window/VRC_Iwsd/Client Starter")]
        static void OpenClientStarterWindow()
        {
            EditorWindow.GetWindow<ClientStarterWindow>("VRC Client");
        }


        private readonly GUIContent startAfterPublished_content
            = new GUIContent("Start After Publish",
                             "Start client automatically after publish completed");
        private readonly GUIContent startLikeAsSDK_content
            = new GUIContent("Use SDK's 'Client Path'",
                             "On: Use 'Installed Client Path' in VRChat SDK Setting to start. Off: Use launch link (vrchat://...) only");
        private readonly GUIContent useNoVrOption_content
            = new GUIContent("Desktop mode",
                             "Start VRChat in desktop mode.");
        private readonly GUIContent openLaunchURL_content
            = new GUIContent("Start Published World",
                             "Starts VRChat client manually");
        private readonly GUIContent openManageURL_content
            = new GUIContent("Open Manage Page",
                             "Open world management page at vrchat.com");
        private readonly GUIContent startAnotherWorld_content
            = new GUIContent(" Start another world:",
                             "To start a world that doesn't relate to editing scene");
        private readonly GUIContent result1_blueprintId_content
            = new GUIContent("World ID (read only)",
                             "Automatically filled with editing scene's ID");
        // private readonly GUIContent _content
        //     = new GUIContent("",
        //                      "");

        bool moreOptions = false;
        string manualInputId = "(input world ID: wrld_xxx...)"; // I choiced not to be saved
        ClientStarter.Result result2 = new ClientStarter.Result(null, true, "");

        void OnGUI ()
        {
            var settings = ClientStarter.settings;
            var result1 = ClientStarter.lastResult;

            /// Label
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("VRChat Client Starter", new GUIStyle(){fontStyle = FontStyle.Bold});
            moreOptions = EditorGUILayout.ToggleLeft("Advanced", moreOptions);
            EditorGUILayout.EndHorizontal();

            /// Settings
            EditorGUI.BeginChangeCheck();
            settings.startAfterPublished = EditorGUILayout.Toggle(startAfterPublished_content, settings.startAfterPublished);
            if (moreOptions)
            {
                settings.startLikeAsSDK = EditorGUILayout.Toggle(startLikeAsSDK_content, settings.startLikeAsSDK);
                GUI.enabled = settings.startLikeAsSDK;
                EditorGUI.indentLevel++;
                settings.useNoVrOption = EditorGUILayout.Toggle(useNoVrOption_content, settings.useNoVrOption);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }
            settings.worldAccessLevel1 = (ClientStarter.WorldAccessLevel)
                EditorGUILayout.EnumPopup("Access", settings.worldAccessLevel1);

            if (EditorGUI.EndChangeCheck()) {
                settings.Store();
            }

            if (moreOptions) // World ID
            {
                // Use PrefixLabel and SelectableLabel instead of TextField.
                // (Is there better way to read only TextField?)
                // EditorGUILayout.TextField("World ID (read only)", result.blueprintId);
                var rect = EditorGUILayout.GetControlRect(true);
                EditorGUI.PrefixLabel(rect, result1_blueprintId_content);
                rect.x += EditorGUIUtility.labelWidth;
                rect.width -= EditorGUIUtility.labelWidth;
                GUI.enabled = false; // To draw fake TextField rect.
                EditorGUI.TextField(rect, "");
                GUI.enabled = true;
                EditorGUI.SelectableLabel(rect, result1.blueprintId);
            }

            /// Operation buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Operations");
            if (GUILayout.Button(openLaunchURL_content, GUILayout.ExpandWidth(false)))
            {
                ClientStarter.lastResult = ClientStarter.TryToOpenLaunchURL(null, settings.worldAccessLevel1);
            }
            if (GUILayout.Button(openManageURL_content, GUILayout.ExpandWidth(false)))
            {
                ClientStarter.lastResult = ClientStarter.TryToOpenManageURL(null);
            }
            if (moreOptions)
            {
                if (GUILayout.Button("Copy Launch Link", GUILayout.ExpandWidth(false)))
                {
                    var r = ClientStarter.ComposeLaunchURL(null, settings.worldAccessLevel1);
                    if (r.IsSucceeded)
                    {
                        EditorGUIUtility.systemCopyBuffer = r.Value;
                    }
                    ClientStarter.lastResult = r;
                }
            }
            EditorGUILayout.EndHorizontal();

            /// Info 1
            EditorGUILayout.Space();
            if (!result1.IsSucceeded)
            {
                EditorGUILayout.HelpBox(result1.Value, MessageType.Warning);
            }

            /// Manual input ID
            if (moreOptions)
            {
                /// Section label
                GUILayout.Space(15);
                EditorGUILayout.LabelField(startAnotherWorld_content, new GUIStyle(){fontStyle = FontStyle.Bold});
                EditorGUI.indentLevel++;

                /// Settings 2
                settings.worldAccessLevel2 = (ClientStarter.WorldAccessLevel)
                    EditorGUILayout.EnumPopup("Access", settings.worldAccessLevel2);

                manualInputId = EditorGUILayout.TextField("World ID", manualInputId);

                /// Operation buttons 2
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Operations");
                if (GUILayout.Button("Start Published World",  GUILayout.ExpandWidth(false)))
                {
                    result2 = ClientStarter.TryToOpenLaunchURL(manualInputId, settings.worldAccessLevel2);
                }
                if (GUILayout.Button("Open Manage Page",  GUILayout.ExpandWidth(false)))
                {
                    result2 = ClientStarter.TryToOpenManageURL(manualInputId);
                }
                if (GUILayout.Button("Copy Launch Link", GUILayout.ExpandWidth(false)))
                {
                    var r = ClientStarter.ComposeLaunchURL(manualInputId, settings.worldAccessLevel2);
                    if (r.IsSucceeded)
                    {
                        EditorGUIUtility.systemCopyBuffer = r.Value;
                    }
                    result2 = r;
                }
                EditorGUILayout.EndHorizontal();

                /// Info 2
                EditorGUILayout.Space();
                if (!result2.IsSucceeded)
                {
                    EditorGUILayout.HelpBox(result2.Value, MessageType.Warning);
                }

                EditorGUI.indentLevel--;
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
            public bool startLikeAsSDK;
            public bool useNoVrOption;
            public WorldAccessLevel worldAccessLevel1;
            public WorldAccessLevel worldAccessLevel2;


            private const string startAfterPublished_key = "Iwsd.ClientStarter.startAfterPublished";
            private const string startLikeAsSDK_key      = "Iwsd.ClientStarter.startLikeAsSDK";
            private const string useNoVrOption_key       = "Iwsd.ClientStarter.useNoVrOption";
            private const string worldAccessLevel1_key   = "Iwsd.ClientStarter.worldAccessLevel1";
            private const string worldAccessLevel2_key   = "Iwsd.ClientStarter.worldAccessLevel2";

            internal static Settings Load()
            {
                var o = new Settings();
                o.startAfterPublished = EditorPrefs.GetBool(startAfterPublished_key, true);
                o.startLikeAsSDK = EditorPrefs.GetBool(startLikeAsSDK_key, true);
                o.useNoVrOption = EditorPrefs.GetBool(useNoVrOption_key, true);
                o.worldAccessLevel1 = (WorldAccessLevel)EditorPrefs.GetInt(worldAccessLevel1_key, (int)WorldAccessLevel.Friends);
                o.worldAccessLevel2 = (WorldAccessLevel)EditorPrefs.GetInt(worldAccessLevel2_key, (int)WorldAccessLevel.Friends);
                return o;
            }

            internal void Store()
            {
                EditorPrefs.SetBool(startAfterPublished_key, this.startAfterPublished);
                EditorPrefs.SetBool(startLikeAsSDK_key, this.startLikeAsSDK);
                EditorPrefs.SetBool(useNoVrOption_key, this.useNoVrOption);
                EditorPrefs.SetInt(worldAccessLevel1_key, (int)this.worldAccessLevel1);
                EditorPrefs.SetInt(worldAccessLevel2_key, (int)this.worldAccessLevel2);
            }
        }

        /**
         * Process rusult value object
         *
         * This is used for not only total result but also partial result,
         * such as getting URL string.
         * So the meaning of Value property vary as processing goes by.
         */
        public class Result {
            public bool IsSucceeded;
            public string Value;
            public string blueprintId;

            public Result(Result result, bool isSucceeded, string value)
            {
                IsSucceeded = isSucceeded;
                Value = value;
                blueprintId = (result == null)? "---": result.blueprintId;
            }
        }

        ////////////////////////////////////////////////////////////
        // Auto start handling

        static public Settings settings;
        static public Result lastResult;

        static ClientStarter()
        {
            lastResult = new Result(null, true, "");
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
                lastResult = TryToOpenLaunchURL(null, settings.worldAccessLevel1);
                if (lastResult.IsSucceeded)
                {
                    completeDialog[0].Close();
                }
            }

        }


        ////////////////////////////////////////////////////////////
        // Do start actions

        private static Result GetVRChatInstalledPath()
        {
            using (var registryKey
                   = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 438100"))
            {
                if (registryKey != null)
                {
                    var val = registryKey.GetValue("InstallLocation").ToString();
                    registryKey.Close();
                    return new Result(null, true, val);
                }
            }
            return new Result(null, false, "Error. (Fail to open registry)");
        }

        private static Result ExtractClientPath(Result url)
        {
            var clientPath = EditorPrefs.GetString("VRC_installedClientPath", "");
            if (clientPath == "")
            {
                return new Result(url, false, "Couldn't get installed client path. (Please check 'Client Path' in 'VRChat Setting')");
            }
            else if (clientPath.StartsWith(@"\"))
            {
                var r = GetVRChatInstalledPath();
                if (!r.IsSucceeded)
                {
                    return r;
                }
                clientPath = r.Value + clientPath;
            }

            return new Result(url, true, clientPath);
        }

        private static Result TryToStart(Result url)
        {
            if (!url.IsSucceeded)
            {
                Debug.LogWarning(url.Value);
                return url;
            }

            var path = ExtractClientPath(url);
            var clientPath = path.Value;

            // ProcessStartInfo.ArgumentList is not available in Unity 2017.4
            // So assemble arguments to one string
            var args = "\"" + url.Value + "\"";
            if (settings.useNoVrOption)
            {
                args = "\"--no-vr\" " + args;
            }

            Debug.Log("will Start path='"+ clientPath + "', args='" + args + "'");

            try
            {
                using (var proc = new System.Diagnostics.Process())
                {
                    var wdir = System.IO.Path.GetDirectoryName(clientPath);
                    var info = proc.StartInfo;
                    info.FileName = clientPath;
                    info.Arguments = args;
                    info.WorkingDirectory = wdir;
                    info.UseShellExecute = true;
                    proc.Start();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Result(url, false, "Couldn't execute process properly. (Please check 'Client Path' in 'VRChat Setting')");
            }

            return url;
        }

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

        public static Result TryToOpenManageURL(string id_opt)
        {
            return TryToOpen(ComposeManageURL(id_opt));
        }

        public static Result TryToOpenLaunchURL(string id_opt, WorldAccessLevel accessLevel)
        {
            var url = ComposeLaunchURL(id_opt, accessLevel);
            return (settings.startLikeAsSDK)? TryToStart(url): TryToOpen(url);
        }


        // "wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        // VRChat use lowercase only
        private static Regex worldIdRex
            = new Regex("wrld_[0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}");

        public static Result ExtractSceneBlueprintId(string id_opt)
        {
            if (id_opt != null)
            {
                var match = worldIdRex.Match(id_opt);
                if (match.Success)
                {
                    var r = new Result(null, true, "");
                    r.blueprintId = match.Value;
                    return r;
                }
                else
                {
                    var r = new Result(null, false, "It's not a world ID string (wrong format)");
                    return r;
                }
            }

#if UNITY_2018_4_OR_NEWER
            var vrcPipelineManager = Resources.FindObjectsOfTypeAll(typeof(VRC.Core.PipelineManager)) as VRC.Core.PipelineManager[];
#else
            var vrcPipelineManager = Resources.FindObjectsOfTypeAll(typeof(VRC.Core.VRCPipelineManager)) as VRC.Core.VRCPipelineManager[];
#endif
            foreach (var pm in vrcPipelineManager)
            {
                // PrefabUtility.GetPrefabType returns None on Unity 2017.4.15f1, PrefabInstance on Unity 2017.4.28f1
                // And GetPrefabType is obsolete in Unity 2018.3.x. Use PrefabUtility.IsPartOfPrefabAsset instead in future.
                PrefabType ptype = PrefabUtility.GetPrefabType(pm);
                if ((ptype == PrefabType.PrefabInstance) || (ptype == PrefabType.None))
                {
                    var blueprintId = pm.blueprintId;
                    if (blueprintId == null || blueprintId == "")
                    {
                        return new Result(null, false, "Not publishd yet? (blueprintId is empty)");
                    }

                    var r = new Result(null, true, "");
                    r.blueprintId = blueprintId;
                    return r;
                }
            }
            return new Result(null, false, "VRC_SceneDescriptor is missing? (vrcPipelineManager.Length=" + vrcPipelineManager.Length + ")");
        }


        public static Result ComposeLaunchURL(string id_opt, WorldAccessLevel accessLevel)
        {
            var access = accessStringOf(accessLevel);

            var bid = ExtractSceneBlueprintId(id_opt);
            if (!bid.IsSucceeded)
            {
                return bid;
            }
            var blueprintId = bid.blueprintId;

            var nonce = Guid.NewGuid();
            var instno = new System.Random().Next(1000, 9000);


            // NOTE 'ref' should be other value. But leave it because the API is undocumented.
            //  "vrchat://launch?ref=vrchat.com&id={blueprintId}:{instno}~{access}({userid})~nonce({nonce}){option}";

            var url = "vrchat://launch?ref=vrchat.com&id=" + blueprintId + ":" + instno;
            if (accessLevel != WorldAccessLevel.Public)
            {
                if (!VRC.Core.APIUser.IsLoggedInWithCredentials)
                {
                    return new Result(bid, false, "Not logged in. (Open 'VRChat SDK/Settings' to check and try again)");
                }

                var user = VRC.Core.APIUser.CurrentUser;
                if (user == null)
                {
                    return new Result(bid, false, "user == null");
                }
                var userid = user.id;
                if (userid == null)
                {
                    return new Result(bid, false, "user.id == null");
                }

                // add more
                url += "~" + access + "("+ userid + ")~nonce(" + nonce + ")";

                if (accessLevel == WorldAccessLevel.InvitePlus)
                {
                    // add more
                    url += "~canRequestInvite";
                }
            }

            return new Result(bid, true, url);
        }

        public static Result ComposeManageURL(string id_opt)
        {
            var bid = ExtractSceneBlueprintId(id_opt);
            if (bid.IsSucceeded)
            {
                // https://vrchat.com/home/world/{blueprintId}
                return new Result(bid, true, "https://vrchat.com/home/world/" + bid.blueprintId);
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
