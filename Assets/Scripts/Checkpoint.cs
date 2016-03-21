using UnityEngine;

namespace Sanicball
{
    public class Checkpoint : MonoBehaviour
    {
        //Data object to hold and hide things that are mostly not changed
        public CheckpointData data;

        public CheckpointToAIPathConnection[] AIPathConnections;

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, GetRespawnPoint());
            Gizmos.DrawSphere(GetRespawnPoint(), 3);
            foreach (var conn in AIPathConnections)
            {
                if (conn.node != null)
                {
                    Gizmos.DrawLine(transform.position, conn.node.transform.position);
                }
            }
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
        public string routeName;
        public AINode node;
        public float risk = 1f;
        public float reward = 1f;
        public bool usedByBig = true;
    }
}
