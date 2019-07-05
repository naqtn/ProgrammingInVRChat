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
 * TODO Save options persistently & let it work even if setting window doesn't opened
 * TODO check what happens avatar publishing, avatar scene case
 * TODO hide "not logged in" warning when IsLoggedInWithCredentials becomes true
 * TODO When ContentUploadedDialog opend and this toggle turn on, close dialog and suppress starting client (Better update hook will solve this issue naturally)
 * TODO Better polling (only just few seconds after play ends)
 * TODO show URL as copyable string (need?)
 * TODO preserve nonce and instance number and reasonably refresh, to meet players by multiple invoke
 * TODO Show warnings HelpBox for open manage operation too.
 * TODO Add non-VR selection feature (need directly starting client instead of Application.OpenURL())
 */
namespace Iwsd
{

    public enum WorldAccessLevel
    {
        Public,      // access "", omit tailing ~ section
        FriendsPlus, // access "hidden"
        Friends,     // access "friends"
        InvitePlus,  // access "private", at last "~canRequestInvite"
        Invite,      // access "private"
    }

    public class ClientStarter : EditorWindow
    {
        [MenuItem("Window/VRC_Iwsd/Client Starter")]
        static void OpenClientStarter()
        {
            EditorWindow.GetWindow<ClientStarter>("VRC Client");
        }

        public bool startAfterPublished = true;

        public WorldAccessLevel worldAccessLevel = WorldAccessLevel.Friends;

        private ComposeResult lastLaunch = new ComposeResult(true, "");

        void OnGUI ()
        {
            //// Label
            EditorGUILayout.LabelField("VRChat Client Starter", new GUIStyle(){fontStyle = FontStyle.Bold});

            //// Settings
            startAfterPublished = EditorGUILayout.Toggle(new GUIContent("Auto Start", "Start client after publish completed"),
                                                         startAfterPublished);
            worldAccessLevel = (WorldAccessLevel)EditorGUILayout.EnumPopup("Access", worldAccessLevel);


            //// Operation buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Operations");
            if (GUILayout.Button("Start Now"))
            {
                lastLaunch = TryToOpenLaunchURL(worldAccessLevel);
            }
            if (GUILayout.Button("Open Manage Page"))
            {
                TryToOpenManageURL();
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();


            //// Info
            if (!lastLaunch.IsSucceeded)
            {
                EditorGUILayout.HelpBox(lastLaunch.Result, MessageType.Warning);
            }
        }


        int callCount;
        void OnEnable ()
        {
            // For manual spike
            // EditorApplication.modifierKeysChanged += PublishPolling;
            // Too early to get ContentUploadedDialog
            // EditorApplication.playmodeStateChanged += PublishPolling;

            // so, use update and polling
            EditorApplication.update += PublishPolling;
            callCount = 0;
        }

        public int CHECK_CYCLE = 200;
        private void PublishPolling()
        {
            // To reduce CPU load
            if (++callCount % CHECK_CYCLE != 0)
            {
                return;
            }
            callCount = 0;

            // TODO hook update only if needed (i.e. setup checked)
            if (!startAfterPublished)
            {
                return;
            }

            var completeDialog = Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.ContentUploadedDialog)) as EditorWindow[];
            if (completeDialog.Length != 0)
            {
                var url = TryToOpenLaunchURL(worldAccessLevel);
                if (url != null)
                {
                    completeDialog[0].Close();
                }
            }

        }


        ////////////////////////////////////////////////////////////////////////////////

        public class ComposeResult {
            public bool IsSucceeded;
            public string Result;
            public ComposeResult(bool isSucceeded, string result)
            {
                IsSucceeded = isSucceeded;
                Result = result;
            }
        }

        private static ComposeResult TryToOpen(ComposeResult url)
        {
            if (url.IsSucceeded)
            {
                var s = url.Result;
                Debug.Log("will OpenURL url='" + s + "'");
                Application.OpenURL(s);
            }
            else
            {
                Debug.LogWarning(url.Result);
            }

            return url;
        }

        public static ComposeResult TryToOpenManageURL()
        {
            return TryToOpen(ComposeManageURL());
        }

        public static ComposeResult TryToOpenLaunchURL(WorldAccessLevel accessLevel)
        {
            return TryToOpen(ComposeLaunchURL(accessLevel));
        }

        public static ComposeResult ExtractSceneBlueprintId()
        {
            var vrcPipelineManager = Resources.FindObjectsOfTypeAll(typeof(VRC.Core.VRCPipelineManager)) as VRC.Core.VRCPipelineManager[];
            foreach (var pm in vrcPipelineManager)
            {
                PrefabType ptype = PrefabUtility.GetPrefabType(pm);
                if (ptype == PrefabType.PrefabInstance)
                {
                    // Debug.Log("Found PrefabInstance");
                    var blueprintId = pm.blueprintId;
                    if (blueprintId == null || blueprintId == "")
                    {
                        return new ComposeResult(false, "Not publishd yet? (blueprintId is empty)");
                    }
                        
                    return new ComposeResult(true, blueprintId);
                }
            }
            return new ComposeResult(false, "VRC_SceneDescriptor missing? (vrcPipelineManager.Length=" + vrcPipelineManager.Length + ")");
        }
        

        public static ComposeResult ComposeLaunchURL(WorldAccessLevel accessLevel)
        {
            var bid = ExtractSceneBlueprintId();
            if (!bid.IsSucceeded)
            {
                return bid;
            }
            var blueprintId = bid.Result;

            if (!VRC.Core.APIUser.IsLoggedInWithCredentials)
            {
                return new ComposeResult(false, "Not logged in. (Open 'VRChat SDK/Settings' )");
            }

            var user = VRC.Core.APIUser.CurrentUser;
            if (user == null)
            {
                return new ComposeResult(false, "user == null");
            }
            var userid = user.id;
            if (userid == null)
            {
                return new ComposeResult(false, "user.id == null");
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

            return new ComposeResult(true, url);
        }

        public static ComposeResult ComposeManageURL()
        {
            var bid = ExtractSceneBlueprintId();
            if (bid.IsSucceeded)
            {
                // https://vrchat.com/home/world/{blueprintId}
                return new ComposeResult(true, "https://vrchat.com/home/world/" + bid.Result);
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
