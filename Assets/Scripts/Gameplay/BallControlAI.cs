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
                Vector3 velocity = GetComponent<Rigidbody>().velocity;
                Quaternion towardsVelocity = (velocity != Vector3.zero) ? Quaternion.LookRotation(velocity) : Quaternion.LookRotation(target.transform.position);

                Ray ray = new Ray(transform.position, towardsVelocity * Vector3.forward);
                float maxDist = Mathf.Min(Vector3.Distance(transform.position, target.transform.position) - 10, velocity.magnitude);

                Vector3 point = transform.position + (ray.direction * maxDist);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDist, LayerMask.GetMask("Terrain")))
                {
                    point = hit.point;
                }

                Vector3 targetPoint = target.transform.position;
                Quaternion directionToGo = Quaternion.LookRotation(point - targetPoint);
                ball.DirectionVector = directionToGo * Vector3.left;

                Debug.DrawLine(point, targetPoint);
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