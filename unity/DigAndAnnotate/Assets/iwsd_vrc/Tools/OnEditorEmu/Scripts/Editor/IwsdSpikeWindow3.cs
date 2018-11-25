using UnityEngine;

using System;
using UnityEditor;

namespace Iwsd
{
    public class IwsdSpikeWindow3 : EditorWindow
    {
        static IwsdSpikeWindow3 instance;

        [MenuItem("VRC_Iwsd/IwsdSpikeWindow3")]
        static void OpenIwsdSpikeWindow3()
        {
            EditorWindow.GetWindow<IwsdSpikeWindow3>("IwsdSpikeWindow3");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI ()
        {
            EditorGUILayout.LabelField("IwsdSpikeWindow 3");
            if (GUILayout.Button("Copy active GameObject to fill some volume"))
            {
                CopyToFill();
            }
        }


        //////////////////////////////

        // ref. http://baba-s.hatenablog.com/entry/2015/05/19/101134
        
        void CopyToFill()
        {
            var active = Selection.activeGameObject;
            
            if (active == null)
            {
                return;
            }

            var t = active;
            for (int zi = 0; zi < 14; zi++)
                for (int yi = 0; yi < 14; yi++) // 18, 22, 8
                    for (int xi = 0; xi < 14; xi++)
                    {
                        var offset = new Vector3(xi*1.0f, yi*1.0f, zi*1.0f);
                        
                        var clone = GameObject.Instantiate(t);
                        var parent = t.transform.parent;
                        clone.transform.SetParent(parent);
                        clone.transform.SetSiblingIndex((parent == null) ? 0 : parent.transform.childCount - 1 );
                        clone.transform.localPosition   = t.transform.localPosition + offset;
                        clone.transform.localRotation   = t.transform.localRotation;
                        clone.transform.localScale      = t.transform.localScale;
                        clone.name = t.name + "_" + xi + "_" + yi + "_" + zi ;
                        // clone.name = GameObjectUtility.GetUniqueNameForSibling( parent, clone.name );
                    }
            
        }
    }
}

