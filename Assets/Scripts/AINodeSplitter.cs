using System.Collections.Generic;
using UnityEngine;

namespace Sanicball
{
    public class AINodeSplitter : AINode
    {
        [SerializeField]
        private AINodeSplitterTarget[] targets;

        public override AINode NextNode
        {
            get
            {
                //Pick a random next node based on their weights
                List<int> choices = new List<int>();
                for (int i = 0; i < targets.Length; i++)
                {
                    for (int j = 0; j < targets[i].Weight; j++) choices.Add(i);
                }
                int randomChoice = Random.Range(0, choices.Count);
                return targets[choices[randomChoice]].Node;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(transform.position, 3f);

            foreach (AINodeSplitterTarget target in targets)
            {
                if (target.Node != null)
                {
                    Gizmos.DrawLine(transform.position, target.Node.transform.position);
                }
            }
        }
    }

    [System.Serializable]
    public class AINodeSplitterTarget
    {
        [SerializeField]
        private AINode node = null;
        [SerializeField]
        private int weight = 1;

        public AINode Node { get { return node; } }
        public int Weight { get { return weight; } }
    }
}