using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Sanicball
{
    [RequireComponent(typeof(BlurOptimized))]
    public class ToggleBlur : MonoBehaviour
    {
        public float speed = 1f;
        private bool isOn = false;
        private float t = 0;
        private BlurOptimized blur;

        public void Toggle()
        {
            isOn = !isOn;
        }

        // Use this for initialization
        private void Start()
        {
            blur = GetComponent<BlurOptimized>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (isOn && t < 1)
            {
                t = Mathf.Lerp(t, 1, speed * Time.deltaTime);
            }
            if (!isOn && t > 0)
            {
                t = 0;
            }
            blur.enabled = t > 0.01f;
            blur.blurSize = Mathf.Lerp(0, 10, t);
            //blur.blurIterations = (int)Mathf.Lerp(0,4,t);
        }
    }
}