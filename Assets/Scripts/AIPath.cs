using System.Collections.Generic;
using UnityEngine;

namespace Sanicball
{
    public class AIPath : MonoBehaviour
    {
        public List<Vector3> nodes;

        private void OnDrawGizmos()
        {
            DrawDotGizmos(Color.blue);
        }

        private void OnDrawGizmosSelected()
        {
            Color c = new Color(0.2f, 0.4f, 1f);
            DrawDotGizmos(c);
            DrawPathGizmos(c);
        }

        private void DrawPathGizmos(Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Gizmos.DrawLine(nodes[i], nodes[i + 1]);
            }
        }

        private void DrawDotGizmos(Color color)
        {
            Gizmos.color = color;
            foreach (var node in nodes)
            {
                Gizmos.DrawSphere(node, 1f);
            }
        }
    }
}
