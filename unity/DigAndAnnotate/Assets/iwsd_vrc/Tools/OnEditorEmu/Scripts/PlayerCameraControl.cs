/*
 * Required GameObject structure
 *
 * character
 *  + this {Camera}
 *     + "HoverAnchor"
 */

/*
 * Mouse input handling is based on [MouseCamLook](https://github.com/jiankaiwang/FirstPersonController)
 *
 * author : jiankaiwang
 * description : The script provides you with basic operations of first personal camera look on mouse moving.
 * platform : Unity
 * date : 2017/12
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Iwsd
{

    public class PlayerCameraControl : MonoBehaviour {

        [SerializeField]
        public float sensitivity = 2.0f;

        [SerializeField]
        public float smoothing = 2.0f;

        // mouse holizontal move to turn left-right
        private  GameObject character;

        private Camera playerCamera;

        private GameObject hoverAnchor;

        // get the incremental value of mouse moving
        // (angle in degrees)
        private Vector2 mouseLook;
        // smooth the mouse moving
        private Vector2 smoothV;

        void Start()
        {
            if (character == null) {
                character = this.transform.parent.gameObject;
            }

            if (playerCamera == null) {
                playerCamera = GetComponent<Camera>();
                if (playerCamera == null) {
                    Iwlog.Error(gameObject, "Camera not found.");
                }
            }

            if (hoverAnchor == null) {
                var tmp = this.transform.Find("HoverAnchor");
                if (tmp == null) {
                    Iwlog.Error(gameObject, "HoverAnchor not found.");
                } else {
                    hoverAnchor = tmp.gameObject;
                }
            }

            // move mouse cursor to center
            Cursor.lockState = CursorLockMode.Locked;
            // Cursor.lockState = CursorLockMode.None;
        }

        // MEMO [raycast basics (jp)](http://megumisoft.hatenablog.com/entry/2015/08/13/172136)

        // Each frame related
        private bool showFocus = false;
        private bool mainOperateButtonDown;
        private bool mainOperateButtonUp;
        private bool subOperateButtonDown;
        private bool subOperateButtonUp;
        
        // pickup related
        // REFINE make a class that handle pickup
        // TODO hold multiple objects (multiple hands)
        VRCSDK2.VRC_Pickup holdingPickup;
        bool originalIsKinematic;
        VRCSDK2.VRC_Pickup.AutoHoldMode AutoHold;
        
        void RayOperation()
        {
            showFocus = false;
            mainOperateButtonDown = Input.GetMouseButtonDown(0);
            mainOperateButtonUp = Input.GetMouseButtonUp(0);
            subOperateButtonDown = Input.GetMouseButtonDown(1);
            subOperateButtonUp = Input.GetMouseButtonUp(1);

            // CHECK precise spec of original UI.
            // - This is "skipping while holding". Is it suitable? 
            // - What happens if use down and drop operation just in one frame? I prefered drop
            // - If both OnInteract and OnPickup are available in a pickable object. in which order they are executed?

            if (holdingPickup == null)
            {
                RayTest();
            }
            else if ((AutoHold == VRCSDK2.VRC_Pickup.AutoHoldMode.Yes)? subOperateButtonDown: mainOperateButtonUp)
            {
                DoDorop(holdingPickup);
            }
            else if ((AutoHold == VRCSDK2.VRC_Pickup.AutoHoldMode.Yes) && (mainOperateButtonDown || mainOperateButtonUp))
            {
                DoUseDownUp(mainOperateButtonDown);
            }
            
            if (!showFocus)
            {
                // just move to where out of sight
                hoverAnchor.transform.position = this.transform.position;
            }
        }

        // TODO show object selection outline
        // memo:
        // http://wiki.unity3d.com/index.php/Silhouette-Outlined_Diffuse
        // [Outline Effect for Unity](https://forum.unity.com/threads/free-open-source-outline-effect.314362/)

        void RayTest()
        {
            var center = new Vector3(Screen.width/2, Screen.height/2, 0);
            Ray ray = playerCamera.ScreenPointToRay(center);

            // TODO Raycast length
            // CHECK Raycast LayerMask
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                ExamineObject(hit.collider.gameObject);
            }
            
            Debug.DrawRay(ray.origin, ray.direction*10, Color.red);
        }
            
        private void ExamineObject(GameObject targetedObject)
        {
            ExamineInteract(targetedObject);
            ExaminePickup(targetedObject);
        }
        
        private void ExamineInteract(GameObject targetedObject)
        {
            // FIXME multiple Trigger components
            // CHECK What happens on original client, if having two valid OnInteract entry with two VRC_Trigger component?
            //   and if with one component (by debug mode Inspector)?
            var triggerComp = targetedObject.GetComponent<Emu_Trigger>();
            if (triggerComp != null)
            {
                // CHECK if broadcast type affects focus outline before interact?
                // (What happens if non-master raycast hit Master broadcast OnInteract object?)
                if (triggerComp.HasTriggerOf(VRCSDK2.VRC_Trigger.TriggerType.OnInteract))
                {
                    showFocus = true;
                    hoverAnchor.transform.position = targetedObject.transform.position;
                
                    // ensure not over an EventSystem object
                    if (mainOperateButtonDown
                        // MEMO when use UI screen && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
                        )
                    {
                        triggerComp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnInteract);
                    }
                }
            }
        }

        private void ExaminePickup(GameObject targetedObject)
        {
            var pickupComp = targetedObject.GetComponent<VRCSDK2.VRC_Pickup>();
            if ((pickupComp != null) && pickupComp.pickupable)
            {
                if (mainOperateButtonDown
                    // MEMO when use UI screen && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
                    )
                {
                    DoPickup(pickupComp);
                }
                else
                {
                    // TODO Change hover text showing it is pickable 
                    // REFINE What kind hover for interact-able and pickup-able object?
                    // (Should I divide operation and view?)
                    showFocus = true;
                    hoverAnchor.transform.position = targetedObject.transform.position;
                }
            }
        }

        private void DoPickup(VRCSDK2.VRC_Pickup pickupComp)
        {
            var rigidbodyComp = pickupComp.GetComponent<Rigidbody>();
            if (rigidbodyComp == null)
            {
                Iwlog.Error(gameObject, "Rigidbody not found.");
                return;
            }
            
            originalIsKinematic = rigidbodyComp.isKinematic;
            rigidbodyComp.isKinematic = true;

            // TODO adjust AutoDetect case
            AutoHold = pickupComp.AutoHold;
            
            holdingPickup = pickupComp;

            var triggerComp = pickupComp.GetComponent<Emu_Trigger>();
            if (triggerComp != null)
            {
                triggerComp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnPickup);
            }
        }

        private void DoDorop(VRCSDK2.VRC_Pickup pickupComp)
        {
            if (pickupComp != holdingPickup)
            {
                return;
            }
            
            var rigidbodyComp = pickupComp.GetComponent<Rigidbody>();
            if (rigidbodyComp == null)
            {
                Iwlog.Error(gameObject, "Rigidbody not found.");
                return;
            }

            rigidbodyComp.isKinematic = originalIsKinematic;
            holdingPickup = null;

            var triggerComp = pickupComp.GetComponent<Emu_Trigger>();
            if (triggerComp != null)
            {
                triggerComp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnDrop);
            }
        }


        private void DoUseDownUp(bool doDown)
        {
            if (holdingPickup == null)
            {
                Iwlog.Error(gameObject, "pickupComp == null");
                return;
            }
            
            var triggerComp = holdingPickup.GetComponent<Emu_Trigger>();
            if (triggerComp != null)
            {
                triggerComp.ExecuteTriggers(doDown?
                                            VRCSDK2.VRC_Trigger.TriggerType.OnPickupUseDown
                                            : VRCSDK2.VRC_Trigger.TriggerType.OnPickupUseUp);
            }
        }

        
        private void UpdatePositionOfHoldings()
        {
            if (holdingPickup == null)
            {
                return;
            }

            // FIXME disolve circular dependancy between PlayerControl.cs and PlayerCameraControl.cs and define player game-objects structure well
            var player = character.GetComponent<PlayerControl>();

            // TODO support PickupOrientation
            var hand = player.RightHoldPosition.transform;
            var obj = holdingPickup.gameObject.transform;
            obj.position = hand.position;
            obj.rotation = hand.rotation;
        }

        
        private void RotateByMouseInput()
        {
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, 1f / smoothing);
            // incrementally add to the camera look
            mouseLook += smoothV;
            mouseLook.x = mouseLook.x % 360.0f;
            mouseLook.y = mouseLook.y % 360.0f;
            
            // mouse up-down => camera up-down
            // vector3.right means the x-axis
            transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            // TODO limit angle

            // REFINE copy and paste
            var player = character.GetComponent<PlayerControl>();
            player.RightArm.transform.localRotation = transform.localRotation;
            
            // mouse left-right => player character lef-right
            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
        }


        // use for TeleportTo
        internal void SetRotation(Quaternion rotation)
        {
            float yRotate = rotation.eulerAngles.y;
            mouseLook.x = yRotate;

            // FIXME quick hack.
            // Start() is not called yet.
            //   PostProcessScene => Player prefab (Object.Instantiate) => MovePlayerToSpawnLocation
            if (character == null) {
                character = this.transform.parent.gameObject;
            }

            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
        }
        
        private void OperateToggleCursorLock()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked)? CursorLockMode.None: CursorLockMode.Locked;
            }
        }
        
        void Update()
        {
            RayOperation();
            RotateByMouseInput();
            OperateToggleCursorLock();
            UpdatePositionOfHoldings();
        }
    }
}
