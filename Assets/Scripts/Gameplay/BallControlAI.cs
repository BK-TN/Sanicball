using Sanicball.Logic;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Gameplay
{
    [RequireComponent(typeof(Ball))]
    public class BallControlAI : MonoBehaviour
    {
        private const float AUTO_RESPAWN_TIME = 6.66f;
        private const float TARGET_OFFSET_CHANGE_TIME = 3.33f;

        private Ball ball;
        private AINode target = null;
        private AISkillLevel skillLevel = AISkillLevel.Average;

        private float targetPointMaxOffset = 10f;
        private Vector3 targetPointOffsetCurrent = Vector3.zero;
        private Vector3 targetPointOffsetGoal = Vector3.zero;
        private float targetPointOffsetChangeTimer = TARGET_OFFSET_CHANGE_TIME;

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
            //Find AI skill level from race manager
            RaceManager raceManager = FindObjectOfType<RaceManager>();
            if (raceManager)
            {
                skillLevel = raceManager.Settings.AISkill;
                switch (skillLevel)
                {
                    case AISkillLevel.Retarded:
                        targetPointMaxOffset = 200;
                        break;

                    case AISkillLevel.Average:
                        targetPointMaxOffset = 20;
                        break;

                    case AISkillLevel.Dank:
                        targetPointMaxOffset = 0f;
                        break;
                }
            }

            //Set initial target
            //Doing this from a RacePlayer, as it's done when changing checkpoints, doesn't work - when RacePlayer's
            //constructor runs, this component has not yet been added to the AI ball yet.
            target = StageReferences.Active.checkpoints[0].FirstAINode;
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

                float maxDist = Mathf.Max(0, Mathf.Min(velocity.magnitude * 0.8f, Vector3.Distance(transform.position, target.transform.position) - 35));

                Vector3 point = transform.position + (ray.direction * maxDist);

                /*RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDist, LayerMask.GetMask("Terrain")))
                {
                    point = hit.point;
                }*/

                Vector3 targetPoint = target.transform.position + targetPointOffsetCurrent;
                Quaternion directionToGo = Quaternion.LookRotation(point - targetPoint);
                ball.DirectionVector = directionToGo * Vector3.left;

                Debug.DrawLine(point, targetPoint, Color.white);
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

            targetPointOffsetCurrent = Vector3.Lerp(targetPointOffsetCurrent, targetPointOffsetGoal, Time.deltaTime);

            targetPointOffsetChangeTimer -= Time.deltaTime;
            if (targetPointOffsetChangeTimer <= 0)
            {
                targetPointOffsetChangeTimer += TARGET_OFFSET_CHANGE_TIME;
                targetPointOffsetGoal = Random.insideUnitSphere * Random.Range(0, targetPointMaxOffset);
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