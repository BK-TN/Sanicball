using UnityEngine;

namespace Sanicball.Gameplay
{
    public class AITarget : MonoBehaviour
    {
        public float stupidness = 100;

        private Vector2 pos;
        private Vector2 velocity;
        private Vector2 target;
        private int timer;

        public Vector3 GetPos()
        {
            return transform.position + new Vector3(pos.x, 0, pos.y);
        }

        // Use this for initialization
        private void Start()
        {
            pos = Vector2.zero;
            target = Random.insideUnitCircle * stupidness;
            timer = Random.Range(50, 200);
        }

        private void FixedUpdate()
        {
            /*velocity += new Vector2(
                Random.Range(-maxVelocityChange,maxVelocityChange),//x
                Random.Range(-maxVelocityChange,maxVelocityChange));//y
            transform.Translate(new Vector3(velocity.x,0,velocity.y));*/
            pos = Vector2.Lerp(pos, target, 0.01f);

            timer--;
            if (timer <= 0)
            {
                target = Random.insideUnitCircle * stupidness;
                timer = Random.Range(50, 200);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + new Vector3(pos.x, 0, pos.y), 2);
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(transform.position + new Vector3(target.x,0,target.y),2);
        }
    }
}
