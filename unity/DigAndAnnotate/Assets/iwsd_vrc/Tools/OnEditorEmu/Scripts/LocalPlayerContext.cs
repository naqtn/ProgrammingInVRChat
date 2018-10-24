using UnityEngine;

using System.Collections.Generic;

namespace Iwsd
{

    // REFINE Make interface and divide implementation to avoid circular reference
    // REFINE protection level of class and methods
    
    public class LocalPlayerContext {

        public static GameObject PlayerGameObject { get; set; }

        private static VRCSDK2.VRC_SceneDescriptor _SceneDescriptor;
        private static Dictionary<string, Material> pathToMaterial;
        private static Dictionary<string, GameObject> pathToPrefabs;
    
        public static VRCSDK2.VRC_SceneDescriptor SceneDescriptor
        {
            // get;
            set {
                _SceneDescriptor = value;

                // REFINE Should I make another? This implementation requires UnityEditor.
                pathToMaterial = makeAssetPathMap<Material>(_SceneDescriptor.DynamicMaterials.ToArray());
                pathToPrefabs = makeAssetPathMap<GameObject>(_SceneDescriptor.DynamicPrefabs.ToArray());
            }
        }

        private static Dictionary<string, T> makeAssetPathMap<T>(T[] objects)
            where T : UnityEngine.Object
        {
            var map = new Dictionary<string, T>();

#if UNITY_EDITOR // This implementation requires UnityEditor.
            foreach (var obj in objects) {
                var p =  UnityEditor.AssetDatabase.GetAssetPath(obj);
                Iwlog.Debug(" path='" + p + "'");
                if (map.ContainsKey(p)) {
                    Iwlog.Warn("Duplicate?: path='" + p + "'");
                } else {
                    map[p] = obj;
                }
            }
#endif
            return map;
        }

            
        ////////////////////////////////////////////////////////////

        // 
        public static void TeleportPlayer(Vector3 position, bool alignRoomToDestination)
        {
            PlayerGameObject.transform.position = position;

            // TODO Check AlignRoomToDestination spec
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
