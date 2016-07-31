using UnityEngine;
using UnityEngine.Networking;

namespace Sanicball
{
	public class LobbyBallSpawner : BallSpawner
    {

        [SerializeField]
        private LobbyPlatform lobbyPlatform = null;

		public Ball SpawnBall(Data.PlayerType playerType, ControlType ctrlType, int character, string nickname, NetworkConnection con)// aqui doy especificaciones del personaje
        {

            if (lobbyPlatform)
            {
                lobbyPlatform.Activate();
            }
            else
            {
                Debug.LogError("LobbyBallSpawner has no lobby platform assigned");
            }

			if(NetworkManager.singleton.isNetworkActive){
				
				GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().tipo = BallType.LobbyPlayer;

				GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().controlTipo = ctrlType;

				GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().personaje = character;

				GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().nickName = nickname;

				return SpawnBallMultiplayer(transform.position, transform.rotation, BallType.LobbyPlayer, ctrlType, character, nickname, con);
		
			}else{

				return SpawnBallLocal(transform.position, transform.rotation, BallType.LobbyPlayer, ctrlType, character, nickname);

			}

			
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}