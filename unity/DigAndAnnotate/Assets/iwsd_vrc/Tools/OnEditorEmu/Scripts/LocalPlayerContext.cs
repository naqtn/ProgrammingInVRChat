using UnityEngine;

using System.Collections.Generic;

namespace Iwsd
{
    // An interface to avoid circular dependency in design.
    // Let LocalPlayerContext independent from other parts (except VRC components and basic tools like logging) 
    interface ILocalPlayer
    {
        void TeleportTo(Transform destination);
    }

    
    // REFINE Make interface and divide implementation to avoid circular reference
    // REFINE protection level of class and methods
    
    public class LocalPlayerContext {

        private static ILocalPlayer LocalPlayer;
        internal static void SetLocalPlayer(ILocalPlayer aPlayer)
        {
            LocalPlayer = aPlayer;
        }

        private static string enableSimulator_key = "Iwsd.OnEditorEmu.EnableSimulator";
        public static bool EnableSimulator {
            #if UNITY_EDITOR
            // initial value null become false
            get {return System.Convert.ToBoolean(UnityEditor.EditorUserSettings.GetConfigValue(enableSimulator_key));}
            set {UnityEditor.EditorUserSettings.SetConfigValue(enableSimulator_key, value.ToString());}
            #else
            get {return true;}
            set {}
            #endif
        }
        
        private static VRCSDK2.VRC_SceneDescriptor _SceneDescriptor;
        private static Dictionary<string, Material> pathToMaterial;
        private static Dictionary<string, GameObject> pathToPrefabs;
    
        internal static VRCSDK2.VRC_SceneDescriptor SceneDescriptor
        {
            // get;
            set {
                _SceneDescriptor = value;

                pathToMaterial = makeAssetPathMap<Material>(_SceneDescriptor.DynamicMaterials);
                pathToPrefabs = makeAssetPathMap<GameObject>(_SceneDescriptor.DynamicPrefabs);
            }
        }

        private static Dictionary<string, T> makeAssetPathMap<T>(List<T> objects)
            where T : UnityEngine.Object
        {
            var map = new Dictionary<string, T>();

            #if UNITY_EDITOR // This implementation requires UnityEditor.
            foreach (T obj in objects) {
                var p =  UnityEditor.AssetDatabase.GetAssetPath(obj);
                Iwlog.Trace("asset path='" + p + "'");
                if (map.ContainsKey(p)) {
                    Iwlog.Warn("Duplicate?: path='" + p + "'");
                } else {
                    map[p] = obj;
                }
            }
            #else
            // REFINE implement asset mapper withdout UnityEditor
            // (Original matching rule is odd. Wait to be cleared...
            // https://vrchat.canny.io/bug-reports/p/setmaterial-action-use-wrong-material-named-similarly )
            Iwlog.Error("Not implemented withdout UNITY_EDITOR case");
            #endif
            
            return map;
        }

        private static int selectSpawnTransformIndex = 0;
        static System.Random random = new System.Random(0);
    
        private static Transform selectSpawnTransform()
        {
            if (_SceneDescriptor.spawns.Length == 0)
            {
                Iwlog.Error("SceneDescriptor spawns has no element");
                // Use SceneDescriptor itself as a replacement
                return _SceneDescriptor.transform;
            }
            
            switch (_SceneDescriptor.spawnOrder)
            {
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrder.First:
                    return _SceneDescriptor.spawns[0];
                        
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrder.Sequential:
                    selectSpawnTransformIndex = selectSpawnTransformIndex++ % _SceneDescriptor.spawns.Length;
                    return _SceneDescriptor.spawns[selectSpawnTransformIndex];
                    
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrder.Random:
                    return _SceneDescriptor.spawns[random.Next(0, _SceneDescriptor.spawns.Length)];
                    
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrder.Demo:
                    // VR_FEATURE
                    // Demo means "move center of room scale to transform position"
                    // CHECK Then, which value does it use?
                    Iwlog.Warn("SpawnOrder=Demo is not supported");
                    return _SceneDescriptor.spawns[0];
                    
                default:
                    Iwlog.Error("Unknown spawnOrder value=" + _SceneDescriptor.spawnOrder);
                    return _SceneDescriptor.spawns[0];
            }
        }
        
        ////////////////////////////////////////////////////////////
        // Public static interface 
        
        public static void MovePlayerToSpawnLocation()
        {
            var destination = selectSpawnTransform();

            // just check value
            switch (_SceneDescriptor.spawnOrientation)
            {
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrientation.Default:
                    break;
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint:
                    break;
                case VRCSDK2.VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint:
                    // VR_FEATURE
                    Iwlog.Warn("SpawnOrientation=AlignRoomWithSpawnPoint is not supported");
                    break;
                default:
                    Iwlog.Error("Unknown spawnOrientation value=" + _SceneDescriptor.spawnOrientation);
                    break;
            }

            LocalPlayer.TeleportTo(destination);
        }

        
        // 
        public static void TeleportPlayer(Transform destination, bool alignRoomToDestination)
        {
            // VR_FEATURE
            Iwlog.Warn("TeleportPlayer AlignRoomToDestination is not supported");

            LocalPlayer.TeleportTo(destination);
        }


        public static Material GetMaterial(string assetPath)
        {
            // REFINE Should I reproduce bug behavior?
            //
            // There's another way using VRC_SceneDescriptor
            // return VRCSDK2.VRC_SceneDescriptor.GetMaterial(name);
            //
            // Or another implementation will be like this:
            //  remove hedding path and extention ".mat" from assetPath an compares to name of DynamicMaterials materials
            //
            // For now. I use whole asset path as a search key. 
            
            if (pathToMaterial.ContainsKey(assetPath)) {
                return pathToMaterial[assetPath];
            }

            // REFINE Should I load material from asset and update SceneDescriptor?
            Iwlog.Error("Unknown material. You might need to build before play. path='" + assetPath + "'");
            // REFINE Is there more proper way to get error material?
            return new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        public static GameObject GetPrefab(string assetPath)
        {
            if (pathToPrefabs.ContainsKey(assetPath)) {
                return pathToPrefabs[assetPath];
            }

            Iwlog.Error("Unknown prefab. path='" + assetPath + "'");
            return null;
        }

    }

}
