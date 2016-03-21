using Sanicball;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Camera))]
    public class OmniCamera : MonoBehaviour
    {
        public Rigidbody target;
        public bool controlTarget;

        [SerializeField]
        private float orbitHeight = 0.5f;
        [SerializeField]
        private float orbitDistance = 4.0f;

        private Quaternion orbitDirection = Quaternion.Euler(0, 0, 0);
        private Quaternion orbitDirectionWithOffset;
        private Vector3 up = Vector3.up;

        private Quaternion orbitDirectionOffset;

        public void SetDirection(Quaternion dir)
        {
            orbitDirection = dir;
        }

        private void Update()
        {
            if (target != null)
            {
                //Rotate the camera towards the velocity of the rigidbody

                //Set the up vector, and make it lerp towards the target's up vector if the target has a Ball
                Vector3 targetUp = Vector3.up;
                Ball bc = target.GetComponent<Ball>();
                if (bc)
                {
                    targetUp = bc.up;
                }
                up = Vector3.Lerp(up, targetUp, Time.deltaTime * 10f);

                //Based on how fast the target is moving, create a rotation bending towards its velocity.
                Quaternion towardsVelocity = (target.velocity != Vector3.zero) ? Quaternion.LookRotation(target.velocity, up) : Quaternion.Euler(0, 0, 0);
                const float maxTrans = 40f;
                Quaternion final = Quaternion.Slerp(orbitDirection, towardsVelocity, Mathf.Max(0, Mathf.Min(target.velocity.magnitude, maxTrans) / maxTrans));
                //final *= orbitDirectionOffset;

                //Lerp towards the final rotation
                orbitDirection = Quaternion.Slerp(orbitDirection, final, Time.deltaTime * 2);

                if (controlTarget)
                {
                    //Look for a BallControlInput and set its look direction
                    BallControlInput bci = target.GetComponent<BallControlInput>();
                    if (bci != null)
                    {
                        bci.lookDirection = orbitDirection;
                        orbitDirectionOffset = bci.OrbitDirectionOffset;
                    }
                }
                //Set camera FOV to get higher with more velocity
                GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, Mathf.Min(60f + (target.velocity.magnitude), 100f), Time.deltaTime * 4);

                orbitDirectionWithOffset = Quaternion.Slerp(orbitDirectionWithOffset, orbitDirection * orbitDirectionOffset, Time.deltaTime * 3);
                transform.position = target.transform.position + Vector3.up * orbitHeight + orbitDirectionWithOffset * (Vector3.back * orbitDistance);
                transform.rotation = orbitDirectionWithOffset;
            }
        }
    }
}