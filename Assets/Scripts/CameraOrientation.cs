using UnityEngine;

namespace Sanicball
{
    public class CameraOrientation : MonoBehaviour
    {
        /// <summary>
        /// Gets a rotation that ignores the roll of the object to use for cameras.
        /// </summary>
        public Quaternion CameraRotation
        {
            get
            {
                return Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, CameraRotation, Vector3.one);
            Gizmos.DrawFrustum(transform.position, 72f, 1000f, 1f, 16f / 9f);
        }
    }
}
