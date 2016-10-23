using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class LocalPlayerInfoBox : MonoBehaviour
    {
        private string[] lines;
        private int currentLine;

        private bool fadingOut = false;
        private float a = 0f;

        [SerializeField]
        private Text text;
        [SerializeField]
        private Image image;

        private const float fadeTime = 0.2f;
        private const float timerMax = 3f;
        private float timer = timerMax;

        private void Awake()
        {
            text.color = new Color(1, 1, 1, 0);
            lines = new[] { "Nice", "Meme" };
            DisplayCurrentLine();
        }

        private void Update()
        {
            if (fadingOut)
            {
                a -= Time.deltaTime / fadeTime;
                text.color = new Color(1, 1, 1, a);
                if (a <= 0)
                {
                    fadingOut = false;
                    currentLine++;
                    if (currentLine >= lines.Length) currentLine = 0;
                    DisplayCurrentLine();
                }
            }
            else if (a < 1)
            {
                a += Time.deltaTime / fadeTime;
                text.color = new Color(1, 1, 1, a);
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer < 0 && lines.Length > 1)
                {
                    fadingOut = true;
                    timer = timerMax;
                }
            }
        }

        public void SetLines(params string[] newLines)
        {
            lines = newLines;
            currentLine = 0;
            timer = timerMax;
            DisplayCurrentLine();
        }

        public void SetIcon(Sprite sprite)
        {
            image.sprite = sprite;
        }

        private void DisplayCurrentLine()
        {
            text.text = lines[currentLine];
        }
    }
}