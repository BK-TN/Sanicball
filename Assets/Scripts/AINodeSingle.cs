using System.Collections.Generic;
using UnityEngine;

namespace Sanicball
{
    public class AINodeSingle : AINode
    {
        [SerializeField]
        private AINode nextNode;

        public override AINode NextNode { get { return nextNode; } }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(transform.position, 3f);

            if (nextNode)
            {
                Gizmos.DrawLine(transform.position, nextNode.transform.position);
            }
        }
    }
}