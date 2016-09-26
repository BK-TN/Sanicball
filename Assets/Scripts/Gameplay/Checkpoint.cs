using UnityEngine;

namespace Sanicball.Gameplay
{
    public class Checkpoint : MonoBehaviour
    {
        //Data object to hold and hide things that are mostly not changed
        [SerializeField]
        private CheckpointData data;

        [SerializeField]
        private AINode firstAINode = null;

        public AINode FirstAINode { get { return firstAINode; } }

        public void Show()
        {
            GetComponent<Renderer>().material = data.matShown;
            data.checkpointMinimap.material.mainTexture = data.texMinimapShown;
        }

        public void Hide()
        {
            GetComponent<Renderer>().material = data.matHidden;
            data.checkpointMinimap.material.mainTexture = data.texMinimapHidden;
        }

        public Vector3 GetRespawnPoint()
        {
            RaycastHit hit;
            Vector3 result = transform.position;
            if (Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out hit, 200, data.ballSpawningMask))
            {
                result = hit.point;
            }
            return result;
        }

        private void Start()
        {
            Hide();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f);
            if (firstAINode != null)
            {
                Gizmos.DrawLine(transform.position, firstAINode.transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, GetRespawnPoint());
            Gizmos.DrawSphere(GetRespawnPoint(), 3);
        }
    }

    [System.Serializable]
    public class CheckpointData
    {
        public Renderer checkpointMinimap;

        public Material matShown;
        public Material matHidden;

        public Texture2D texMinimapShown;
        public Texture2D texMinimapHidden;

        public LayerMask ballSpawningMask;
    }

    [System.Serializable]
    public class CheckpointToAIPathConnection
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private AINode firstNode;
        [SerializeField]
        private float selectionWeight = 1f;
        [SerializeField]
        private bool usedByBig = true;

        public AINode FirstNode { get { return firstNode; } }
        public float SelectionWeight { get { return selectionWeight; } }
        public bool UsedByBig { get { return usedByBig; } }
    }
}