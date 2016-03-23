using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Ball))]
    public class BallControlInput : MonoBehaviour
    {
        private Ball ball;
        private Vector3 rawDirection;
        private bool hasJumped = false;

        public Quaternion CameraDirectionOffset { get; private set; }
        public Quaternion LookDirection { get; set; }

        private void Start()
        {
            LookDirection = Quaternion.Euler(Vector3.forward);
            ball = GetComponent<Ball>();
        }

        private void Update()
        {
            if (UI.PauseMenu.GamePaused) return; //Short circuit if paused

            //GO FAST
            const float weight = 0.5f;

            var targetVector = GameInput.MovementVector(ball.CtrlType);
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
            directionVector = LookDirection * Quaternion.Euler(0f, 90f, 0f) * directionVector; //Multiply vector by camera rotation
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

            //ROTATE CAMERA FAST
            Vector2 camVector = GameInput.CameraVector(ball.CtrlType);
            Vector3 orientedCamVector = new Vector3(camVector.x, 0, camVector.y);
            if (orientedCamVector != Vector3.zero)
            {
                Quaternion camQuaternion = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(orientedCamVector), orientedCamVector.magnitude);
                CameraDirectionOffset = camQuaternion;
            }
            else
            {
                CameraDirectionOffset = Quaternion.identity;
            }

            //RESPAWN FAST
            if (GameInput.IsRespawning(ball.CtrlType))
            {
                ball.RequestRespawn();
            }
        }
    }
}