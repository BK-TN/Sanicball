using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Text))]
    public class TextSlideshow : MonoBehaviour
    {
        private Text t;

        [SerializeField]
        private string[] lines;
        [SerializeField]
        private float fadeTime = 0.2f;
        [SerializeField]
        private float waitTime = 3f;

        private int currentLine;
        private bool fadingOut = false;
        private float a = 0f;
        private float timer;

        private void Start()
        {
            timer = waitTime;
            t = GetComponent<Text>();
            t.text = lines[currentLine];
        }

        private void Update()
        {
            if (PauseMenu.GamePaused) return; //Do nothing when paused to prevent text changing annoyingly below the Match Settings window.

            if (fadingOut)
            {
                a -= Time.deltaTime / fadeTime;
                t.color = new Color(1, 1, 1, a);
                if (a <= 0)
                {
                    fadingOut = false;
                    currentLine++;
                    if (currentLine >= lines.Length) currentLine = 0;
                    t.text = lines[currentLine];
                }
            }
            else if (a < 1)
            {
                a += Time.deltaTime / fadeTime;
                t.color = new Color(1, 1, 1, a);
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer < 0 && lines.Length > 1)
                {
                    fadingOut = true;
                    timer = waitTime;
                }
            }
        }
    }
}