using UnityEngine;

namespace Sanicball.Gameplay
{
    [RequireComponent(typeof(Ball))]
    public class BallControlAI : MonoBehaviour
    {
        private const float AUTO_RESPAWN_TIME = 6.66f;

        private Ball ball;
        private AINode target = null;
        private float autoRespawnTimer = AUTO_RESPAWN_TIME;

        public AINode Target { get { return target; } set { target = value; autoRespawnTimer = AUTO_RESPAWN_TIME; } }

        private void TriggerJump()
        {
            ball.Jump();
        }

        // Use this for initialization
        private void Start()
        {
            ball = GetComponent<Ball>();
            //Set initial target
            //Doing this from a RacePlayer, as it's done when changing checkpoints, doesn't work - when RacePlayer's
            //constructor runs, this component has not yet been added to the AI ball yet.
            target = Logic.StageReferences.Active.checkpoints[0].FirstAINode;
        }

        // Update is called once per frame
        private void Update()
        {
            ball.Brake = false;
            if (target)
            {
                Quaternion towardsTargetDir = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(target.transform.position - transform.position);
                Quaternion finalDir = towardsTargetDir;

                Vector3 velocity = GetComponent<Rigidbody>().velocity;
                if (velocity != Vector3.zero)
                {
                    Quaternion currentDir = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
                    finalDir = Quaternion.LerpUnclamped(currentDir, towardsTargetDir, 1.5f);
                }

                Vector3 finalDirVector = finalDir.eulerAngles;
                finalDir = Quaternion.Euler(0, finalDirVector.y, finalDirVector.z); //We don't want to rotate around the x axis (Causes the ball to spin)
                //Vector3 moveDir2 = moveDir.eulerAngles;
                //moveDir = Quaternion.Euler(0, moveDir2.y, moveDir2.z);
                ball.DirectionVector = finalDir * Vector3.forward;
            }

            if (ball.CanMove)
            {
                autoRespawnTimer -= Time.deltaTime;
                if (autoRespawnTimer <= 0)
                {
                    ball.RequestRespawn();
                    autoRespawnTimer = AUTO_RESPAWN_TIME;
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            AINode node = other.GetComponent<AINode>();
            if (node && node == target && target.NextNode)
            {
                Target = target.NextNode;
            }
        }
    }
}