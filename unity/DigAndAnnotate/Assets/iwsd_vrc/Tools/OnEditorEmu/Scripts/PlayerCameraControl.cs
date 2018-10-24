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
        private Vector2 mouseLook;
        // smooth the mouse moving
        private Vector2 smoothV;

        private int operateMouseButton = 0;
        
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

        bool mousePressed = false;
        void RayTest()
        {
            var center = new Vector3(Screen.width/2, Screen.height/2, 0);
            Ray ray = playerCamera.ScreenPointToRay(center);

            // TODO Raycast length
            // CHECK Raycast LayerMask
            RaycastHit hit;
            bool hitTrigger = false;
            if (Physics.Raycast(ray, out hit)) {

                var triggerComp = hit.collider.GetComponent<Emu_Trigger>();
                if (triggerComp != null) {
                    // TODO check interactable
                    hoverAnchor.transform.position = hit.collider.transform.position;
                    hitTrigger = true;

                    // ensure not over an EventSystem object
                    if (!mousePressed && Input.GetMouseButtonDown(operateMouseButton)
                        // TODO when use UI screen && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
                        ) {

                        mousePressed = true;
                        triggerComp.OnInteract();
                    }

                }
            }
            if (!hitTrigger) {
                hoverAnchor.transform.position = this.transform.position; // out of sight
            }

            if (!Input.GetMouseButtonDown(operateMouseButton)) {
                mousePressed = false;
            }

            Debug.DrawRay(ray.origin, ray.direction*10, Color.red);

        }

        // TODO show object selection outline
        // memo:
        // http://wiki.unity3d.com/index.php/Silhouette-Outlined_Diffuse
        // [Outline Effect for Unity](https://forum.unity.com/threads/free-open-source-outline-effect.314362/)


        void RotateByMouseInput()
        {
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, 1f / smoothing);
            // incrementally add to the camera look
            mouseLook += smoothV;

            // mouse up-down => camera up-down
            // vector3.right means the x-axis
            transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            // TODO limit angle
        
            // mouse left-right => player character lef-right
            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
        }

        private bool tabLastPress = false;
        private void OperateToggleCursorLock()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                if (!tabLastPress) {
                    tabLastPress = true;
                    Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked)? CursorLockMode.None: CursorLockMode.Locked;
                }
            } else {
                tabLastPress = false;
            }
        }
        
        void Update()
        {
            RayTest();
            RotateByMouseInput();
            OperateToggleCursorLock();
        }
    }
}
