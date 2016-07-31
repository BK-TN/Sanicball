using UnityEngine;
using UnityEngine.Networking;
namespace Sanicball
{
	public abstract class BallSpawner : NetworkBehaviour
    {

        [SerializeField]
		public  Ball ballPrefab = null;
		public Ball pelota;

		BallType tip;
		ControlType contol;
		int cha;
		string nik;

		protected Ball SpawnBallMultiplayer(Vector3 position, Quaternion rotation, BallType ballType, ControlType ctrlType, int character, string nickname, NetworkConnection cons)
        {

			Debug.Log("Starting SpawnMultiplayer method" +  character);

			Debug.Log("Server?: "  + isServer);

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby" && 
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.replace ==false){
				Debug.Log("Adding Player");
				ClientScene.AddPlayer(cons,0 );
			}else{
				Debug.Log("We start a replacement"  +  cons.isReady);
				if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){
					pelota = (Sanicball.Ball)Instantiate(ballPrefab, transform.position + new Vector3(0,4,0), transform.rotation);
				}else{
					pelota = (Sanicball.Ball)Instantiate( FindObjectOfType<RaceBallSpawner>().ballPrefab ,  FindObjectOfType<RaceBallSpawner>().transform.position,
						FindObjectOfType<RaceBallSpawner>().transform.rotation);
				}
				Debug.Log("Ball instantiated");
				if(isServer)
					NetworkServer.ReplacePlayerForConnection(  cons  , pelota.gameObject, 0   ); // ESTO FUNCIOONA
				else{
					pelota.CmdSpawnMe();
				}
			}

			Ball ball;
			ball = pelota;

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){
				// in Lobby we send the data of the ball to All clients, 
				// in Race the RPC happend at the moment to CountDown start. in RaceManager.cs
				pelota.RpcInit(ballType, ctrlType, character, nickname);
			}
            return ball;
        }


		protected Ball SpawnBallLocal(Vector3 position, Quaternion rotation, BallType ballType, ControlType ctrlType, int character, string nickname)
		{
			Debug.Log("This is used to start AI Balls");
			Ball ball = (Ball)Instantiate(ballPrefab, position, rotation);
			ball.Init(ballType, ctrlType, character, nickname);
			NetworkServer.Spawn(ball.gameObject);
			return ball;
		}



		/*
		[ClientRpc]
		public void RpcAsignBall(GameObject pelotin){			
			pelota= pelotin.GetComponent<Ball>();
		}
		*/

    }
}