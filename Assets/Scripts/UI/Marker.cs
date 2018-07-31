using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class Marker : MonoBehaviour
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Text textField;

        public Transform Target { get { return target; } set { target = value; } }
        public string Text { get { return textField.text; } set { textField.text = value; } }
        public Sprite Sprite { set { image.sprite = value; } }
        public Color Color { set { image.color = value; colorAlpha = value.a; } }
        public Camera CameraToUse { get; set; }
        public bool Clamp { get; set; }
        public bool HideImageWhenInSight { get; set; }

        private RectTransform rectTransform;
        private Image image;
        private float colorAlpha;

        private const float clampMin = 0.2f;
        private const float clampMax = 1 - clampMin;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            colorAlpha = image.color.a;
        }

        private void Update()
        {
            var cam = Camera.main;
            if (CameraToUse) cam = CameraToUse;

            if (!target || !cam)
            {
                Destroy(gameObject);
                return;
            }

            var relativePosition = cam.transform.InverseTransformPoint(target.position);
            relativePosition.z = Mathf.Max(relativePosition.z, 1);

            Vector3 p = cam.WorldToViewportPoint(cam.transform.TransformPoint(relativePosition));

            if (Clamp)
                p = new Vector3(Mathf.Clamp(p.x, clampMin, clampMax), Mathf.Clamp(p.y, clampMin, clampMax), p.z);

            rectTransform.anchorMin = p;
            rectTransform.anchorMax = p;

            if (HideImageWhenInSight)
            {
                Ray viewportRay = cam.ViewportPointToRay(p);
                RaycastHit hit;
                bool show = true;
                if (Physics.Raycast(viewportRay, out hit, 400))
                {
                    show = hit.transform == Target;
                }
                image.color = new Color(image.color.r, image.color.g, image.color.b, show ? colorAlpha : 0);
            }
        }
    }
}