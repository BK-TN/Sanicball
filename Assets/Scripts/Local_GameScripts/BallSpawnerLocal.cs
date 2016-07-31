using UnityEngine;

namespace Sanicball
{
	public abstract class BallSpawnerLocal : MonoBehaviour
	{
		[SerializeField]
		private BallLocal ballPrefab = null;

		protected BallLocal SpawnBall(Vector3 position, Quaternion rotation, BallType ballType, ControlType ctrlType, int character, string nickname)
		{
			var ball = (BallLocal)Instantiate(ballPrefab, position, rotation);
			ball.Init(ballType, ctrlType, character, nickname);

			return ball;
		}
	}
}