#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

namespace Iwsd
{

    public class InUnityDebug
    {

        static string playerPrefabPath = "iwsd_vrc/Tools/OnEditorEmu/Prefabs/Emu_Player";
        static string quickMenuPrefabPath = "iwsd_vrc/Tools/OnEditorEmu/Prefabs/Emu_QuickMenu";

        // This is entry point of this emulator.
        [PostProcessScene]
        static void OnPostProcessScene()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode || !Application.isEditor) {
                // CHECK Unity spec. Is this condition suitable?
                return;
            }

            if (!LocalPlayerContext.EnableSimulator)
            {
                // REFINE Is there smart way to alternate between simulator and VRC SDK uploading .
                return;
            }
    
            // NOTE https://anchan828.github.io/editor-manual/web/callbacks.html
            // EditorSceneManager.GetSceneManagerSetup()
        
            Setup_SceneDescriptor();
            Setup_TriggersComponents();

            SpawnPlayerObject();
        }

        static private void Setup_SceneDescriptor()
        {
            var comps = Object.FindObjectsOfType(typeof(VRCSDK2.VRC_SceneDescriptor));

            VRCSDK2.VRC_SceneDescriptor descriptor;
            switch (comps.Length)
            {
                case 0:
                    Iwlog.Warn("VRC_SceneDescriptor not found. Create temporary");
                    var go = new GameObject("VRC_SceneDescriptor holder");
                    descriptor = go.AddComponent<VRCSDK2.VRC_SceneDescriptor>();
                    var scene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MoveGameObjectToScene(go, scene);
                    break;
                case 1:
                    descriptor = (VRCSDK2.VRC_SceneDescriptor)comps[0];
                    break;
                default:
                    Iwlog.Warn("Too many VRC_SceneDescriptor found.");
                    descriptor = (VRCSDK2.VRC_SceneDescriptor)comps[0];
                    break;
            }

            LocalPlayerContext.SceneDescriptor = descriptor;
        }


        // (see also. Execute_SpawnObject)
        static private void Setup_TriggersComponents()
        {
            foreach (VRCSDK2.VRC_Trigger triggerComp in UnityEngine.Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.VRC_Trigger)))
            {
                // Skip VRC_Trigger in prefab asset
                // CHECK Do I have to skip ?
                // 
                // https://answers.unity.com/questions/218429/how-to-know-if-a-gameobject-is-a-prefab.html
                // (In latest Unity GetPrefabParent is obsolete.)
                bool isPrefabOriginal = (PrefabUtility.GetPrefabParent(triggerComp.gameObject) == null)
                    && (PrefabUtility.GetPrefabObject(triggerComp.gameObject.transform) != null);
 
                if (isPrefabOriginal) {
                    continue;
                }
                
                // Emu_Trigger find brother VRC_Trigger by itself
                var emu_trigger = triggerComp.gameObject.AddComponent<Emu_Trigger>();
 
                emu_trigger.debugString = triggerComp.gameObject.name;
            }
        }

        ////////////////////////////////////////////////////////////
        
        static private GameObject SpawnFromPrefab(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Iwlog.Error("Prefab not found. path='" + path + "'");
                return null;
            }
            
            var instance = Object.Instantiate(prefab);
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MoveGameObjectToScene(instance, scene);

            return instance;
        }
        
        static private bool SetupQuickMenu(PlayerControl playerCtrl)
        {
            var quickMenu = SpawnFromPrefab(quickMenuPrefabPath);
            if (!quickMenu)
            {
                return false;
            }

            var canvasObj = quickMenu.transform.Find("Canvas");
            if (!canvasObj) {
                Iwlog.Error("QuickMenu Canvas object not found.");
                return false;
            }
            var canvas = canvasObj.GetComponent<UnityEngine.Canvas>();
            if (!canvas) {
                Iwlog.Error("QuickMenu Canvas component not found.");
                return false;
            }
            
            var camera = playerCtrl.PlayerCamera.GetComponent<Camera>();
            canvas.worldCamera = camera;

            playerCtrl.QuickMenu = quickMenu;

            // initial state is inactive
            quickMenu.SetActive(false);

            return true;
        }


        static private bool SpawnPlayerObject()
        {
            var playerInstance = SpawnFromPrefab(playerPrefabPath);
            if (!playerInstance)
            {
                return false;
            }

            var playerCtrl = playerInstance.GetComponent<PlayerControl>();
            if (!playerCtrl)
            {
                Iwlog.Error("PlayerPrefab must have PlayerControl component");
                return false;
            }
            LocalPlayerContext.SetLocalPlayer(playerCtrl);

            SetupQuickMenu(playerCtrl);
            
            LocalPlayerContext.MovePlayerToSpawnLocation();

            return true;
        }

    }
}

#endif // if UNITY_EDITOR
