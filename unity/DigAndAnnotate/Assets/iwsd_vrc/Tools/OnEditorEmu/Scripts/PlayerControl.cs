using UnityEngine;
using System.Collections;


namespace Iwsd
{

    public class PlayerControl : MonoBehaviour, ILocalPlayer
    {
        // TODO Use values from VRC_SceneDescriptor
        private float speed = 4.0F;
        private float jumpSpeed = 5.0F;
        private float gravity = 20.0F;

        public GameObject PlayerCamera;
        public GameObject RightArm;
        public GameObject RightHoldPosition;
        public GameObject QuckMenuInitLoc;
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

        // TODO goto spawn point
        // see VRC_SceneDescriptor
        
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

            ToggleQuickMenuOperation();
            MovePlayerOperation();
        }


        ////////////////////////////////////////////////////////////

        void MovePlayerOperation()
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


        // Toggle menu
        // "Cancel" is usually bind to ESC
        // Alternative bind : LeftShift + C
        // (In UnitEditor ESC activates cursor automatically. you can avoid this behavior by using alternative bind.)
        void ToggleQuickMenuOperation()
        {
            if (Input.GetButtonUp("Cancel")
                #if UNITY_EDITOR
                || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
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
