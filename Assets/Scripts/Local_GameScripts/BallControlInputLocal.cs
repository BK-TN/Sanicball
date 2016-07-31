using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(BallLocal))]
    public class BallControlInputLocal : MonoBehaviour
    {
        private BallLocal ball;
        private Vector3 rawDirection;
        private bool hasJumped = false;

        public Quaternion LookDirection { get; set; }

        private void Start()
        {
            LookDirection = Quaternion.Euler(Vector3.forward);
            ball = GetComponent<BallLocal>();
        }

        private void Update()
        {
			if (UI.PauseMenu.GamePaused ) return; //Short circuit if paused

            //GO FAST
            const float weight = 0.5f;

			Vector3 targetVector = GameInput.MovementVector(ball.CtrlType);

            rawDirection = Vector3.MoveTowards(rawDirection, targetVector, weight);
            Vector3 directionVector = rawDirection;

            if (directionVector != Vector3.zero)
            {
                //Modify direction vector to be more controller-friendly (And normalize it)
                var directionLength = directionVector.magnitude;
                directionVector = directionVector / directionLength;
                directionLength = Mathf.Min(1, directionLength);
                directionLength = directionLength * directionLength;
                directionVector = directionVector * directionLength;
            }
            directionVector = LookDirection * Quaternion.Euler(0f, 90f, 0) * directionVector; //Multiply vector by camera rotation	
            ball.DirectionVector = directionVector;

            //BRAKE FAST
            ball.Brake = GameInput.IsBraking(ball.CtrlType);

            //JUMP FAST
            if (GameInput.IsJumping(ball.CtrlType))
            {
                if (!hasJumped)
                {
                    ball.Jump();
                    hasJumped = true;
                }
            }
            else
            {
                if (hasJumped)
                    hasJumped = false;
            }

            //RESPAWN FAST
            if (GameInput.IsRespawning(ball.CtrlType) && ball.CanMove)
            {
                ball.RequestRespawn();
            }

			//Switch Camera Fast
			if (GameInput.IsSwitchingCameras(ball.CtrlType) && ball.CanMove)
			{
				ball.SwitchCameraBall();
			}
        }
    }
}