using UnityEngine;
using UnityEngine.Networking;

namespace Sanicball
{
    public class RaceBallSpawner : BallSpawner
    {
        //These two are only used for visualizing spawn positions in the editor
        [SerializeField]
        private int editorBallCount = 8;

        [SerializeField]
        private float editorBallSize = 1;

        [SerializeField]
        private int columns = 10;

        [SerializeField]
        private LayerMask ballSpawningMask = new LayerMask();

		public Ball SpawnBall(int position, BallType ballType, ControlType ctrlType, int character, string nickname, NetworkConnection con )
        {
            float characterSize = Data.ActiveData.Characters[character].ballSize;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().tipo = ballType;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().controlTipo = ctrlType;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().personaje = character;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().nickName = nickname;

			if(ballType == BallType.Player){
				return SpawnBallMultiplayer(GetSpawnPoint(position, characterSize / 2f), transform.rotation, ballType, ctrlType, character, nickname,con);

			}else 
				return SpawnBallLocal(GetSpawnPoint(position, characterSize / 2f), transform.rotation, ballType, ctrlType, character, nickname);

        }

		public Ball SpawnBall(int position, BallType ballType, ControlType ctrlType, int character, string nickname )
		{
			float characterSize = Data.ActiveData.Characters[character].ballSize;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().tipo = ballType;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().controlTipo = ctrlType;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().personaje = character;

			GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().nickName = nickname;

			return SpawnBallLocal(GetSpawnPoint(position, characterSize / 2f), transform.rotation, ballType, ctrlType, character, nickname);

		}


        public Vector3 GetSpawnPoint(int position, float offsetY)
        {
            //Get the row of the ball
            int row = position / columns;

            Vector3 dir;
            if (position % 2 == 0)
            {
                dir = Vector3.right * ((position % columns) / 2 + 0.5f) * 2;
            }
            else {
                dir = Vector3.left * ((position % columns) / 2 + 0.5f) * 2;
            }
            dir += (Vector3.back * 2f) * row;

            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(dir + Vector3.up * 100), Vector3.down, out hit, 200, ballSpawningMask))
            {
                dir = transform.InverseTransformPoint(hit.point);
            }
            return transform.TransformPoint(dir) + Vector3.up * offsetY;
        }

        private void Start()
        {
            //Disable the arrow gizmo
            GetComponent<Renderer>().enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1);
            columns = (int)Mathf.Max(1, columns);
            for (int i = 0; i < editorBallCount; i++)
            {
                Gizmos.DrawSphere(GetSpawnPoint(i, editorBallSize / 2f), editorBallSize / 2f);
            }
        }
    }
}