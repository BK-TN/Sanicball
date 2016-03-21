using UnityEngine;

namespace Sanicball.UI
{
    public class RaceCountdown : MonoBehaviour
    {
        private int countdown = 5;
        private float timer = 2f;

        private float currentFontSize = 60;
        private float targetFontSize = 60;

        [SerializeField]
        private AudioClip countdown1;

        [SerializeField]
        private AudioClip countdown2;

        [SerializeField]
        private UnityEngine.UI.Text countdownLabel;

        public event System.EventHandler OnCountdownFinished;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                string countdownText = "";
                int countdownFontSize = 60;

                countdown--;
                switch (countdown)
                {
                    case 4:
                        countdownText = "READY";
                        countdownFontSize = 80;
                        UISound.Play(countdown1);
                        break;

                    case 3:
                        countdownText = "STEADY";
                        countdownFontSize = 100;
                        UISound.Play(countdown1);
                        break;

                    case 2:
                        countdownText = "GET SET";
                        countdownFontSize = 120;
                        UISound.Play(countdown1);
                        break;

                    case 1:
                        countdownText = "GO FAST";
                        countdownFontSize = 160;
                        UISound.Play(countdown2);
                        if (OnCountdownFinished != null)
                            OnCountdownFinished(this, new System.EventArgs());
                        break;

                    case 0:
                        Destroy(gameObject);
                        break;
                }

                countdownLabel.text = countdownText;
                targetFontSize = countdownFontSize;

                timer = 1f;
                if (countdown == 1) timer = 2f;
            }

            currentFontSize = Mathf.Lerp(currentFontSize, targetFontSize, Time.deltaTime * 10);
            countdownLabel.fontSize = (int)currentFontSize;
        }
    }
}
