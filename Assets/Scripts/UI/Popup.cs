using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sanicball.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Popup : MonoBehaviour
    {
        public GameObject firstSelectedOnOpen;
        public Action onClose;
        private CanvasGroup cg;
        private float alpha = 0f;
        private float fadeTime = 0.2f;

        private bool closing = false;

        public void Close()
        {
            closing = true;
            if (onClose != null)
            {
                onClose();
            }
        }

        private void Start()
        {
            cg = GetComponent<CanvasGroup>();
            cg.alpha = 0f;

            if (firstSelectedOnOpen)
            {
                var es = FindObjectOfType<EventSystem>();
                if (es)
                    es.SetSelectedGameObject(firstSelectedOnOpen);
            }
        }

        private void Update()
        {
            if (!closing && alpha < 1f)
            {
                alpha = Mathf.Min(alpha + Time.deltaTime / fadeTime, 1f);
                cg.alpha = alpha;
            }
            if (closing && alpha > 0f)
            {
                alpha = Mathf.Max(alpha - Time.deltaTime / fadeTime, 0f);
                cg.alpha = alpha;
                if (alpha <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
