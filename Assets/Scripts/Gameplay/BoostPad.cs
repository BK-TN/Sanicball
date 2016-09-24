using Sanicball;
using UnityEngine;

namespace Sanicball.Gameplay
{
    public class BoostPad : MonoBehaviour
    {
        public float speed = 1f;

        private float offset;

        private void Update()
        {
            //Animate the panel on the boost pad
            //TODO: Move this to seperate component
            offset -= 5f * Time.deltaTime;
            if (offset <= 0f)
            {
                offset += 1f;
            }
            GetComponent<Renderer>().materials[1].SetTextureOffset("_MainTex", new Vector2(0f, offset));
        }

        private void OnTriggerEnter(Collider other)
        {
            var bc = other.GetComponent<Ball>();
            if (bc != null)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb)
                {
                    float speed = rb.velocity.magnitude;
                    speed += this.speed;
                    rb.velocity = transform.rotation * Vector3.forward * speed;
                }
            }
        }
    }
}