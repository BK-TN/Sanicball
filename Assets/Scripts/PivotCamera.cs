using System;
using System.Collections;
using UnityEngine;

namespace Sanicball
{
    public class PivotCamera : MonoBehaviour, IBallCamera
    {
        public Rigidbody Target { get; set; }
        public Camera Cam { get; set; }

        public Camera attachedCamera;
        public Vector3 defaultCameraPosition = new Vector3(6, 2.8f, 0);
        public float cameraDistance = 1;
        public float cameraDistanceTarget = 1;

        public void SetDirection(Quaternion dir)
        {
            transform.rotation = dir;
        }

        private void FixedUpdate()
        {
            //if (Input.GetKey(KeyCode.LeftArrow)) {
            //	transform.Rotate(Vector3.up*3);
            //}
            //if (Input.GetKey(KeyCode.RightArrow)) {
            //	transform.Rotate(Vector3.down*3);
            //}
        }

        private void Start()
        {
            Cam = attachedCamera;
        }

        private void Update()
        {
            var bci = Target.GetComponent<BallControlInput>();
            if (bci)
            {
                bci.LookDirection = transform.rotation * Quaternion.Euler(0, -90, 0);
            }
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = Target.transform.position;

            cameraDistanceTarget = Mathf.Clamp(cameraDistanceTarget - (Input.GetAxis("Mouse ScrollWheel") * 2), 0, 10);
            cameraDistance = Mathf.Lerp(cameraDistance, cameraDistanceTarget, Time.deltaTime * 4);

            //Ground collision
            /*int layer = 1 << LayerMask.NameToLayer("Terrain"); //Select terrain layer
            Vector3 relativeCameraPos = transform.rotation * defaultCameraPosition;
            float distance = Vector3.Distance(Vector3.zero,relativeCameraPos);

            Ray ray = new Ray(transform.position,relativeCameraPos.normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance, layer)) {
                attachedCamera.transform.position = hit.point;
            } else {*/
            Vector3 targetPoint = defaultCameraPosition * cameraDistance;

            attachedCamera.transform.position = transform.TransformPoint(targetPoint);
            //}
        }
    }
}