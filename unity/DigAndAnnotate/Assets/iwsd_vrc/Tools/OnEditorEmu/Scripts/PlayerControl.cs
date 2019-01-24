using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Iwsd
{

    public class PlayerControl : MonoBehaviour, ILocalPlayer
    {
        // TODO Use values from VRC_SceneDescriptor
        private float speed = 4.0F;
        private float jumpSpeed = 5.0F;
        private float gravity = 20.0F;

        // REFINE How to define player game-objects structure. see also PlayerCameraControl.cs 
        public GameObject PlayerCamera;
        public GameObject RightArm;
        public GameObject RightHoldPosition;
        public GameObject EventSystemHolder;
        public GameObject QuckMenuInitLoc;

        // Quick Menu object comes from a prefab
        // REFINE Is it needed to define quick menu solely (or separately) in other prefab ?
        internal GameObject QuickMenu;
        
        private Vector3 moveDirection = Vector3.zero;

        private CharacterController controller;

        private void SetupCamera()
        {
            var orgMainCam = GameObject.FindWithTag("MainCamera");
            if (orgMainCam != null) {
                orgMainCam.SetActive(false);
            }

            // TODO use VRC_SceneDescriptor.ReferenceCamera as template
            if (PlayerCamera == null) {
                Iwlog.Error(gameObject, "PlayerCamera not specified.");
            } else {
                PlayerCamera.SetActive(true);
            }
        
        }

        // CHECK Is CharacterController good enough?
        // [Unity: CHARACTER CONTROLLER vs RIGIDBODY](https://medium.com/ironequal/unity-character-controller-vs-rigidbody-a1e243591483)
    

        void Start()
        {
            controller = GetComponent<CharacterController>();

            SetupCamera();
        }


        void Update()
        {
            // TODO Shift to run
            // TODO respawn hight

            CheckEvnetSystem();
            ToggleQuickMenuOperation();
            MovePlayerOperation();
        }

        
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            foreach (var triggerComp in hit.gameObject.GetComponents<Emu_Trigger>())
            {
                triggerComp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnAvatarHit);
            }
        }


        ////////////////////////////////////////////////////////////

        private void MovePlayerOperation()
        {
            if (controller.isGrounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                }
            }
            moveDirection.y -= gravity * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);
        }


        private void CheckEvnetSystem()
        {
            // Disable other EventSystem.
            // Though Unity warns as "Multiple EventSystems in scene... this is not supported",
            // after that our EventSystem starts to work.
            
            if ((EventSystem.current != null) && (EventSystem.current.gameObject != EventSystemHolder))
            {
                EventSystem.current.enabled = false;
                var theEventSystem = EventSystemHolder.GetComponent<EventSystem>();
                // To call OnEnable()
                theEventSystem.enabled = false;
                theEventSystem.enabled = true;
            }
        }

        
        // Toggle quick menu
        // "Cancel" is usually bind to ESC
        // Alternative bind : Q
        // (In UnitEditor ESC activates mouse cursor automatically. You can avoid this behavior by using alternative bind.)
        private void ToggleQuickMenuOperation()
        {
            if (Input.GetButtonUp("Cancel")
                #if UNITY_EDITOR
                || Input.GetKeyDown(KeyCode.Q)
                #endif
                )
            {
                Iwlog.Trace("Toggle QuickMenu");
                if (QuickMenu.activeSelf)
                {
                    QuickMenu.SetActive(false);
                }
                else
                {
                    QuickMenu.transform.position = QuckMenuInitLoc.transform.position;
                    QuickMenu.transform.rotation = QuckMenuInitLoc.transform.rotation;
                    QuickMenu.SetActive(true);
                }
            }
        }

        
        ////////////////////////////////////////////////////////////
        // LocalPlayer implementation
        
        public void TeleportTo(Transform destination)
        {
            this.transform.position = destination.position;

            // FIXME disolve circular dependancy between PlayerControl.cs and PlayerCameraControl.cs
            // (Though here, it's fine that PlayerControl depends PlayerCameraControl.)
            var ctrl = PlayerCamera.GetComponent<PlayerCameraControl>();
            ctrl.SetRotation(destination.rotation);
        }
    }

}
