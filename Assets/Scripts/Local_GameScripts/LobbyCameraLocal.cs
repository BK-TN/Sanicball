using System.Collections.Generic;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Camera))]
    public class LobbyCameraLocal : MonoBehaviour
    {
        public float rotationSpeed;

        private Quaternion startRotation;
        private Quaternion targetRotation;

        private List<BallLocal> ballsLocal = new List<BallLocal>();

        public void AddBall(BallLocal b)
        {
			ballsLocal.Add(b);
        }

        private void Start()
        {
            startRotation = transform.rotation;
        }

        private void Update()
        {
			if (ballsLocal.Count > 0)
            {
                Vector3 sum = Vector3.zero;

                //Copy the balls array for safe modification
				var ballsCopy = new List<BallLocal>(ballsLocal);
                foreach (var b in ballsCopy)
                {
                    //Check for removed balls
                    if (b == null)
                    {
						ballsLocal.Remove(b);
                        continue;
                    }
                    //Add position to sum
                    sum += b.transform.position;

                    if (b.Input)
                    {
                        b.Input.LookDirection = transform.rotation;
                    }
                }
                //Divide sum by number of balls to get the average position (<3 you vector math)
                var target = sum / ballsCopy.Count;

                //Rotate towards target point
                targetRotation = Quaternion.LookRotation(target - transform.position);
            }
            else
            {
                //Rotate towards default orientation with no players
                targetRotation = startRotation;
            }

            //Rotate
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}