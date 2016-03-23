using System.Collections;
using Sanicball.Data;
using UnityEngine;

namespace Sanicball
{
    public class SmoothMouseLook : MonoBehaviour
    {
        public bool canControl = true;
        public int smoothing = 2;
        public int min = -85;
        public int max = 85;

        private float xtargetRotation = 90;
        private float ytargetRotation = 0;
        private float sensitivityMouse = 3;
        private float sensitivityKeyboard = 10;

        // Use this for initialization
        private void Start()
        {
            sensitivityMouse = ActiveData.GameSettings.oldControlsMouseSpeed;
            sensitivityKeyboard = ActiveData.GameSettings.oldControlsKbSpeed;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !GameInput.KeyboardDisabled)
            {
                if (canControl)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (canControl)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    float yAxisMove = Input.GetAxis("Mouse Y") * sensitivityMouse; // how much has the mouse moved?
                    ytargetRotation += -yAxisMove; // what is the target angle of rotation

                    float xAxisMove = Input.GetAxis("Mouse X") * sensitivityMouse; // how much has the mouse moved?
                    xtargetRotation += xAxisMove; // what is the target angle of rotation
                }

                //Keyboard controls
                if (Input.GetKey(ActiveData.Keybinds[Keybind.CameraLeft]))
                    xtargetRotation -= 10 * sensitivityKeyboard * Time.deltaTime;
                if (Input.GetKey(ActiveData.Keybinds[Keybind.CameraRight]))
                    xtargetRotation += 10 * sensitivityKeyboard * Time.deltaTime;
                if (Input.GetKey(ActiveData.Keybinds[Keybind.CameraUp]))
                    ytargetRotation -= 10 * sensitivityKeyboard * Time.deltaTime;
                if (Input.GetKey(ActiveData.Keybinds[Keybind.CameraDown]))
                    ytargetRotation += 10 * sensitivityKeyboard * Time.deltaTime;

                ytargetRotation = Mathf.Clamp(ytargetRotation, min, max);
                xtargetRotation = xtargetRotation % 360;
                ytargetRotation = ytargetRotation % 360;

                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, xtargetRotation, ytargetRotation), Time.deltaTime * 10 / smoothing);
                //transform.localRotation=Quaternion.Lerp(transform.parent.rotation,Quaternion.Euler(0,xtargetRotation,0),Time.deltaTime*10/smoothing);
            }
        }

        public void SetTargetRotation(float xRot, float yRot)
        {
            xtargetRotation = xRot;
            ytargetRotation = yRot;
        }

        public float GetXTargetRotation()
        {
            return xtargetRotation;
        }

        public float GetYTargetRotation()
        {
            return ytargetRotation;
        }

        public Quaternion GetTargetDirection()
        {
            return Quaternion.Euler(0, xtargetRotation, ytargetRotation);
        }
    }
}