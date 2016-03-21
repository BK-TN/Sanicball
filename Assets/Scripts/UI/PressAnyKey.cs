using UnityEngine;
using UnityEngine.Events;

namespace Sanicball.UI
{
    public class PressAnyKey : MonoBehaviour
    {
        public UnityEvent onAnyKeyPressed;

        public float timer = 10f;
        private float spin = 0f;

        private void Update()
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }
            else {
                if (spin == 0f) spin = 0.01f;
                if (spin < 1000000f)
                {
                    spin += Time.deltaTime * spin;
                }
            }
            transform.Rotate(0f, 0f, spin * Time.deltaTime);

            if (Input.anyKeyDown)
            {
                onAnyKeyPressed.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
