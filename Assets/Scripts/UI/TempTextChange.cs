using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Text))]
    public class TempTextChange : MonoBehaviour
    {
        public float time = 1f;
        private float t = 0f;

        private Text text;

        private string prevText;

        public void Activate(string newText)
        {
            text.text = newText;
            t = time;
        }

        private void Start()
        {
            text = GetComponent<Text>();
            prevText = text.text;
        }

        private void Update()
        {
            if (t > 0f)
            {
                t -= Time.deltaTime;
                if (t <= 0)
                {
                    text.text = prevText;
                }
            }
        }
    }
}
