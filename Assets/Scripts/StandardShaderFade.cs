using UnityEngine;

namespace Sanicball
{
    public class StandardShaderFade : MonoBehaviour
    {
        private float targetTime;
        private float curTime;

        private bool fadeOut = false;

        public void FadeIn(float time)
        {
            targetTime = time;
            curTime = 0f;
            fadeOut = false;
        }

        public void FadeOut(float time)
        {
            targetTime = time;
            curTime = 0f;
            fadeOut = true;
        }

        private void Start()
        {
            //FadeOut(5f);
        }

        private void Update()
        {
            if (curTime < targetTime)
            {
                curTime = Mathf.Min(curTime + Time.deltaTime, targetTime);
                GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, fadeOut ? (1 - curTime / targetTime) : (curTime / targetTime)));
            }
        }
    }
}
