using UnityEngine;

namespace Sanicball
{
    public class AINode : MonoBehaviour
    {
        public AINode nextNode;

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, 1f);
            if (nextNode != null)
            {
                Gizmos.DrawLine(transform.position, nextNode.transform.position);
            }
        }
    }
}
