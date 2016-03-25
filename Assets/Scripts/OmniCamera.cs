using Sanicball;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Camera))]
    public class OmniCamera : MonoBehaviour, IBallCamera
    {
        public Rigidbody Target { get; set; }
        public Camera AttachedCamera
        {
            get
            {
                if (!attachedCamera)
                {
                    attachedCamera = GetComponent<Camera>();
                }
                return attachedCamera;
            }
        }
        private Camera attachedCamera;

        [SerializeField]
        private float orbitHeight = 0.5f;
        [SerializeField]
        private float orbitDistance = 4.0f;

        private Quaternion currentDirection = Quaternion.Euler(0, 0, 0);
        private Quaternion currentDirectionWithOffset = Quaternion.Euler(0, 0, 0);
        private Quaternion directionOffset;
        private Vector3 up = Vector3.up;

        public void SetDirection(Quaternion dir)
        {
            currentDirection = dir;
        }

        private void Update()
        {
            if (Target != null)
            {
                //Rotate the camera towards the velocity of the rigidbody

                //Set the up vector, and make it lerp towards the target's up vector if the target has a Ball
                Vector3 targetUp = Vector3.up;
                Ball bc = Target.GetComponent<Ball>();
                if (bc)
                {
                    targetUp = bc.Up;
                }
                up = Vector3.Lerp(up, targetUp, Time.deltaTime * 10);

                //Based on how fast the target is moving, create a rotation bending towards its velocity.
                Quaternion towardsVelocity = (Target.velocity != Vector3.zero) ? Quaternion.LookRotation(Target.velocity, up) : Quaternion.identity;
                const float maxTrans = 20f;
                Quaternion finalTargetDir = Quaternion.Slerp(currentDirection, towardsVelocity, Mathf.Max(0, Mathf.Min(Target.velocity.magnitude, maxTrans) / maxTrans));

                //Lerp towards the final rotation
                currentDirection = Quaternion.Slerp(currentDirection, finalTargetDir, Time.deltaTime * 2);

                //Look for a BallControlInput and set its look direction
                BallControlInput bci = Target.GetComponent<BallControlInput>();
                if (bci != null)
                {
                    bci.LookDirection = currentDirection;
                    directionOffset = bci.CameraDirectionOffset;
                }

                //Set camera FOV to get higher with more velocity
                AttachedCamera.fieldOfView = Mathf.Lerp(AttachedCamera.fieldOfView, Mathf.Min(60f + (Target.velocity.magnitude), 100f), Time.deltaTime * 4);

                currentDirectionWithOffset = Quaternion.Slerp(currentDirectionWithOffset, currentDirection * directionOffset, Time.deltaTime * 3);
                transform.position = Target.transform.position + Vector3.up * orbitHeight + currentDirectionWithOffset * (Vector3.back * orbitDistance);
                transform.rotation = currentDirectionWithOffset;
            }
        }
    }
}