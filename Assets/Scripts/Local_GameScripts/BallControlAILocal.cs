using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(BallLocal))]
    public class BallControlAILocal : MonoBehaviour
    {
        public AITarget aiTargetPrefab;
        public int stupidness = 30;
        public Path pathToFollow;
        public AITarget target;

        private BallLocal ballControl;

        public void TriggerJump()
        {
            ballControl.Jump();
        }

        // Use this for initialization
        private void Start()
        {
            ballControl = GetComponent<BallLocal>();
            //PathToFollow
            pathToFollow = GameObject.FindWithTag("AIPath").GetComponent<Path>();
        }

        // Update is called once per frame
        private void Update()
        {
            ballControl.Brake = false;
            Quaternion moveDir = Quaternion.LookRotation(target.GetPos() - transform.position);
            Vector3 moveDir2 = moveDir.eulerAngles;
            moveDir = Quaternion.Euler(0, moveDir2.y, moveDir2.z);
            ballControl.DirectionVector = Quaternion.Euler(0, 90, 0) * moveDir * Vector3.forward;
            //Debug.DrawRay(transform.position,moveDir*directionVector*100);
            //rigidbody.AddTorque((Quaternion.Euler(0,90,0)*moveDir*directionVector)*rollSpeed);
        }
    }
}