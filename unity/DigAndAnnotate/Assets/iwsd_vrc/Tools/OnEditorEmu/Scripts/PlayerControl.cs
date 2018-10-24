using UnityEngine;
using System.Collections;


namespace Iwsd
{

    public class PlayerControl : MonoBehaviour {
    
        public float speed = 4.0F;
        public float jumpSpeed = 5.0F;
        public float gravity = 20.0F;

        public GameObject playerCamObj;
    
        private Vector3 moveDirection = Vector3.zero;

        private CharacterController controller;

        private void SetupCamera()
        {
            var orgMainCam = GameObject.FindWithTag("MainCamera");
            if (orgMainCam != null) {
                orgMainCam.SetActive(false);
            }

            // TODO use VRC_SceneDescriptor.ReferenceCamera as template
            if (playerCamObj == null) {
                Iwlog.Error(gameObject, "playerCamObj not specified.");
            } else {
                playerCamObj.SetActive(true);
            }
        
        }

        // TODO goto spawn point
        // see VRC_SceneDescriptor
        
        // CHECK Is CharacterController good enough?
        // [Unity: CHARACTER CONTROLLER vs RIGIDBODY](https://medium.com/ironequal/unity-character-controller-vs-rigidbody-a1e243591483)
    

        void Start() {
            controller = GetComponent<CharacterController>();

            SetupCamera();
        }
    
        void Update() {
            
            // TODO ESC to menu
            // TODO Shift to run
            // TODO respawn hight
            
            if (controller.isGrounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (Input.GetButton("Jump"))
                    moveDirection.y = jumpSpeed;
            
            }
            moveDirection.y -= gravity * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);
        }
    }

}
