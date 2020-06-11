#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

namespace Iwsd
{

    public class InUnityDebug
    {

        private const string playerPrefabPath = "iwsd_vrc/Tools/OnEditorEmu/Prefabs/Emu_Player";
        private const string quickMenuPrefabPath = "iwsd_vrc/Tools/OnEditorEmu/Prefabs/Emu_QuickMenu";

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
            Setup_ReplaceTriggerRefference();

            Setup_AudioListeners();
            
            SpawnPlayerObject();
        }

        ////////////////////////////////////////////////////////////

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

        // Suppress messages for AudioListeners
        // "There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene."
        // PlayerCamera is to be expected to add later.
        static private void Setup_AudioListeners()
        {
            foreach (UnityEngine.AudioListener comp in Object.FindObjectsOfType(typeof(UnityEngine.AudioListener)))
            {
                comp.enabled = false;
            }
        }

        static private void Setup_VRC_UiShape(Camera playerCamera)
        {
            foreach (VRCSDK2.VRC_UiShape comp in UnityEngine.Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.VRC_UiShape)))
            {

                Canvas foundCanvas = comp.GetComponent<Canvas>();
                if (foundCanvas)
                {
                    foundCanvas.worldCamera = playerCamera;
                }
            }
        }

        ////////////////////////////////////////////////////////////

        // https://answers.unity.com/questions/218429/how-to-know-if-a-gameobject-is-a-prefab.html
        // (In latest Unity GetPrefabParent is obsolete.)
        static private bool IsInPrefab(Component comp)
        {
            return (PrefabUtility.GetPrefabParent(comp.gameObject) == null)
                && (PrefabUtility.GetPrefabObject(comp.gameObject.transform) != null);
        }

        // (see also. Execute_SpawnObject)
        static private void Setup_TriggersComponents()
        {
            foreach (VRCSDK2.VRC_Trigger triggerComp in UnityEngine.Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.VRC_Trigger)))
            {
                // Skip VRC_Trigger in prefab asset
                // CHECK Do I have to skip ?
                if (IsInPrefab(triggerComp))
                {
                    continue;
                }

                // Emu_Trigger find brother VRC_Trigger by itself
                var emu_trigger = triggerComp.gameObject.GetOrAddComponent<Emu_Trigger>();

                emu_trigger.debugString = triggerComp.gameObject.name;
            }
        }

        static private void Setup_ReplaceTriggerRefference()
        {
            int procCount = 0;
            procCount += ReplaceTriggerRefferenceInUIEvent(typeof(UnityEngine.UI.Button), "m_OnClick");
            procCount += ReplaceTriggerRefferenceInUIEvent(typeof(UnityEngine.UI.InputField), "m_OnValueChanged");
            procCount += ReplaceTriggerRefferenceInUIEvent(typeof(UnityEngine.UI.InputField), "m_OnEndEdit");
            Iwlog.Debug("VRC_Trigger references in UIEvents replaced for runtime. Count=" + procCount);
        }

        static private int ReplaceTriggerRefferenceInUIEvent(System.Type UIComponentType, string EventPropName)
        {
            int procCount = 0;
            foreach (Component comp in UnityEngine.Resources.FindObjectsOfTypeAll(UIComponentType))
            {
                if (ReplaceTriggerRefferenceInUIEvent(comp, EventPropName))
                {
                    procCount++;
                }
            }
            return procCount;
        }

        static private bool ReplaceTriggerRefferenceInUIEvent(Component comp, string EventPropName)
        {
            if (IsInPrefab(comp)) // CHECK Do I have to skip ? see. Setup_TriggersComponents
            {
                return false;
            }

            int procCount = 0;

            // To modify persistent part of UnityEventBase, we must use persistent API.
            var sComp = new SerializedObject(comp);

            var uiEvent = sComp.FindProperty(EventPropName);
            var m_PersistentCalls = uiEvent.FindPropertyRelative("m_PersistentCalls");
            var m_Calls = m_PersistentCalls.FindPropertyRelative("m_Calls");
            for (int idx = m_Calls.arraySize - 1; 0 <= idx; idx--)
            {
                var persCall = m_Calls.GetArrayElementAtIndex(idx);
                var m_Target = persCall.FindPropertyRelative("m_Target");
                UnityEngine.Object targetObj = m_Target.objectReferenceValue;

                if ((targetObj != null) && (targetObj.GetType() == typeof(VRCSDK2.VRC_Trigger)))
                {
                    var emuTrigger = ((Component)targetObj).GetComponent(typeof(Emu_Trigger));
                    if (emuTrigger == null)
                    {
                        Iwlog.Error("emuTrigger == null");
                    }
                    else
                    {
                        m_Target.objectReferenceValue = emuTrigger;
                        procCount++;
                    }
                }
            }
            if (0 < procCount)
            {
                sComp.ApplyModifiedPropertiesWithoutUndo();
            }

            return (0 < procCount);
        }

        // UIEvent SerializedObject structure memo.
        // in case of Button
        // MonoBehaviour:   <= Button component
        //   ...
        //   m_OnClick:         <= inherited from UnityEvent
        //     m_PersistentCalls:
        //       m_Calls:           <= List<PersistentCall>
        //       - m_Target: {fileID: 878055904}
        //         m_MethodName: set_layer



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

            Setup_VRC_UiShape(playerCtrl.PlayerCamera.GetComponent<Camera>());

            SetupQuickMenu(playerCtrl);

            LocalPlayerContext.MovePlayerToSpawnLocation();

            return true;
        }

    }
}

#endif // if UNITY_EDITOR
