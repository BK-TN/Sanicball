using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Ball))]
    public class BallControlInput : MonoBehaviour
    {
        public Quaternion lookDirection;
        private Ball ball;
        private Vector3 rawDirection;
        private bool hasJumped = false;

        public Quaternion OrbitDirectionOffset { get; set; }

        private void Start()
        {
            lookDirection = Quaternion.Euler(Vector3.forward);
            ball = GetComponent<Ball>();
        }

        private void Update()
        {
            if (UI.PauseMenu.GamePaused) return; //Short circuit if paused

            //GO FAST
            //Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            float weight = 0.5f;

            var targetVector = GameInput.MovementVector(ball.controlType);

            rawDirection = Vector3.MoveTowards(rawDirection, new Vector3(targetVector.x, 0, targetVector.y), weight);

            Vector3 directionVector = rawDirection;

            if (directionVector != Vector3.zero)
            { //Modify direction vector to be more controller-friendly (And normalize it)
                var directionLength = directionVector.magnitude;
                directionVector = directionVector / directionLength;
                directionLength = Mathf.Min(1, directionLength);
                directionLength = directionLength * directionLength;
                directionVector = directionVector * directionLength;
            }

            //Quaternion cameraDir;
            //if (cameraPivot != null)
            //	cameraDir = cameraPivot.GetTargetDirection();
            //else
            //	cameraDir = Quaternion.Euler(0,90,0);

            directionVector = lookDirection * Quaternion.Euler(0f, 90f, 0f) * directionVector; //Multiply vector by camera rotation

            ball.directionVector = directionVector;

            //BRAKE FAST
            ball.brake = GameInput.IsBraking(ball.controlType);

            //JUMP FAST
            //TODO: ball seems to jump really high if checking for button held. investigate pls
            if (GameInput.IsJumping(ball.controlType))
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
            Vector2 camVector = GameInput.CameraVector(ball.controlType);
            Vector3 orientedCamVector = new Vector3(camVector.x,0,camVector.y);
            if (orientedCamVector != Vector3.zero)
            {
                Quaternion camQuaternion = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(orientedCamVector), orientedCamVector.magnitude);
                OrbitDirectionOffset = camQuaternion;
            }
            else
            {
                OrbitDirectionOffset = Quaternion.identity;
            }

            //Respawning
            if (GameInput.IsRespawning(ball.controlType))
            {
                ball.RequestRespawn();
            }

            if (GameInput.IsOpeningMenu(ball.controlType))
            {
                //Debug.Log("Opening menu");
            }
        }
    }
}