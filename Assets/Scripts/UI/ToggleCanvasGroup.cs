using System.Collections;
using UnityEngine;

namespace Sanicball.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToggleCanvasGroup : MonoBehaviour
    {
        public bool startHidden = false;
        public float time = 1f;

        private bool hidden = false;

        private CanvasGroup cg;

        private float hideTimer = 0f;

        public bool Hidden { get { return hidden; } }

        public void Toggle()
        {
            if (hidden) Show(); else Hide();
        }

        public void Show()
        {
            hidden = false;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            gameObject.SetActive(true);
        }

        public void ShowTemporarily(float time)
        {
            Show();
            hideTimer = time;
        }

        public void Hide()
        {
            StartCoroutine(Hide2());
        }

        private IEnumerator Hide2()
        {
            yield return null;
            hidden = true;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        private void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            hidden = startHidden;
            cg.alpha = hidden ? 0f : 1f;
            cg.interactable = !hidden;
            cg.blocksRaycasts = !hidden;
            if (hidden)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (hidden && cg.alpha > 0f)
            {
                cg.alpha = Mathf.Max(cg.alpha - Time.deltaTime / time, 0f);
                if (cg.alpha <= 0f)
                {
                    gameObject.SetActive(false);
                }
            }
            if (!hidden && cg.alpha < 1f)
            {
                cg.alpha = Mathf.Min(cg.alpha + Time.deltaTime / time, 1f);
            }

            if (hideTimer > 0f)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    Hide();
                }
            }
        }
    }
}