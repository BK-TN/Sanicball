using UnityEngine;

namespace Sanicball
{
	public class LobbyBallSpawnerLocal : BallSpawnerLocal
	{
		[SerializeField]
		private LobbyPlatform lobbyPlatform = null;

		public BallLocal SpawnBall(Data.PlayerType playerType, ControlType ctrlType, int character, string nickname)
		{
			if (lobbyPlatform)
			{
				lobbyPlatform.Activate();
			}
			else
			{
				Debug.LogError("LobbyBallSpawner has no lobby platform assigned");
			}

			return SpawnBall(transform.position, transform.rotation, BallType.LobbyPlayer, ctrlType, character, nickname);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, 0.5f);
			Gizmos.DrawWireSphere(transform.position, 1f);
		}
	}
}