using UnityEngine;
using UnityEngine.EventSystems;

namespace Iwsd
{

    public class MousePositionFixedInputModule : StandaloneInputModule
    {
        public class FixedInput : BaseInput
        {
            public override Vector2 mousePosition
            {
                get {
                    return new Vector2(Screen.width/2, Screen.height/2);
                }
            }
        }

        override protected void Awake()
        {
            base.Awake();
            // m_InputOverride is not documented.
            // This is equivalent to BaseInputModule.inputOverride that is introduced in later version.
            // ( https://docs.unity3d.com/ScriptReference/EventSystems.BaseInputModule-inputOverride.html )
            m_InputOverride = gameObject.AddComponent<FixedInput>();
        }
        
        public override void Process()
        {
            // PointerInputModule ignores interaction while lockState is CursorLockMode.Locked.
            // (see https://bitbucket.org/Unity-Technologies/ui/src/378bd9240df3ac323ac3de9274dd381ec99f9ebe/UnityEngine.UI/EventSystem/InputModules/PointerInputModule.cs?at=5.6&fileviewer=file-view-default#PointerInputModule.cs-194 )
            // (PointerInputModule is indirect super class of this class.)
            // 
            // We use world space canvas to realize "Spatial UI" or "Diegetic UI". And there is no movable mouse cursor.
            // (https://unity3d.com/learn/tutorials/topics/virtual-reality/user-interfaces-vr)
            // So, temporary change lockState to None to make PointerInputModule working.

            CursorLockMode save = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            base.Process();
            Cursor.lockState = save;
        }

    }
}
