using UnityEngine;

namespace Sanicball
{
    public class LobbyBallSpawner : BallSpawner
    {
        [SerializeField]
        private LobbyPlatform lobbyPlatform = null;

        public Ball SpawnBall(Data.PlayerType playerType, ControlType ctrlType, int character, string nickname)
        {
            if (lobbyPlatform)
            {
                lobbyPlatform.Activate();
            }
            else
            {
                Debug.LogError("LobbyBallSpawner has no lobby platform assigned");
            }

            return SpawnBall(transform.position, transform.rotation, BallType.LobbyPlayer, playerType, ctrlType, character, nickname);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
