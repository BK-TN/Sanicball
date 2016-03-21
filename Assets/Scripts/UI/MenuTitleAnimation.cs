using UnityEngine;

namespace Sanicball.UI
{
    public class MenuTitleAnimation : MonoBehaviour
    {
        public Vector2 positionChange;
        public Vector2 sizeChange;
        public float time = 0.5f;
        private RectTransform rectTransform;
        private Vector2 startPosition;
        private Vector2 startSize;

        private float pos = 0f;
        private bool on = false;

        public void Begin()
        {
            on = true;
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            startPosition = rectTransform.anchoredPosition;
            startSize = rectTransform.sizeDelta;
        }

        private void Update()
        {
            if (on && pos < 1f)
            {
                pos = Mathf.Min(1f, pos + Time.deltaTime / time);

                var smoothedPos = Mathf.SmoothStep(0f, 1f, pos);

                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, startPosition + positionChange, smoothedPos);
                rectTransform.sizeDelta = Vector2.Lerp(startSize, startSize + sizeChange, smoothedPos);
            }
        }
    }
}
