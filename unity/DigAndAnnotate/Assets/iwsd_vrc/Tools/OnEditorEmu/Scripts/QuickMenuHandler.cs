using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Iwsd
{
    class QuickMenuHandler : MonoBehaviour
    {
        public void ExecuteRespawn()
        {
            LocalPlayerContext.MovePlayerToSpawnLocation();
        }

        public void ExecuteQuit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        public void ExecuteChangeColliderSetup(string name)
        {
            LocalPlayerContext.ChangeColliderSetup(name);
        }
    }
}
