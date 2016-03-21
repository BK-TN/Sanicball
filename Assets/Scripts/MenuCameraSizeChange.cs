using UnityEngine;

namespace Sanicball
{
    public class MenuCameraSizeChange : MonoBehaviour
    {
        public float time = 0.5f;
        public float menuWidth = 400f;
        public Canvas canvas;

        private bool resized = false;
        private float pos = 0f;

        public void Resize()
        {
            resized = true;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (resized)
            {
                pos = Mathf.Min(1f, pos + Time.deltaTime / time);

                var smoothedPos = Mathf.SmoothStep(0f, 1f, pos);
                var targetWidth = menuWidth * canvas.scaleFactor;

                Camera.main.rect = new Rect(0, 0, 1f - (smoothedPos * targetWidth) / Screen.width, 1);
            }
        }
    }
}
