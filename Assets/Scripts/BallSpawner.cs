using UnityEngine;

namespace Sanicball
{
    public abstract class BallSpawner : MonoBehaviour
    {
        [SerializeField]
        private Ball ballPrefab = null;

        protected Ball SpawnBall(Vector3 position, Quaternion rotation, BallType ballType, Data.PlayerType playerType, ControlType ctrlType, int character, string nickname)
        {
            var ball = (Ball)Instantiate(ballPrefab, position, rotation);
            ball.type = ballType;
            ball.playerType = playerType;
            ball.controlType = ctrlType;
            ball.character = character;
            ball.nickname = nickname;

            return ball;
        }
    }
}
