using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PopupGameJoltLink : MonoBehaviour
    {
        public Slideshow slideshow;

        public InputField username;
        public InputField token;
        public Text linkError;

        public int loginFieldsSlide;
        public int verifyingSlide;

        private OptionsPanel optionsPanel;

        public void LinkAccount()
        {
            //If both fields empty, clear data
            if (string.IsNullOrEmpty(username.text) && string.IsNullOrEmpty(token.text))
            {
                optionsPanel.SetGameJoltInfo("", "");
                linkError.text = "Link cleared.";
                return;
            }

            if (string.IsNullOrEmpty(token.text))
            {
                linkError.text = "Token field is empty.";
                return;
            }
            if (string.IsNullOrEmpty(username.text))
            {
                linkError.text = "Username field is empty.";
                return;
            }

            slideshow.SetSlide(verifyingSlide);

            GJAPI.Users.Verify(username.text, token.text);
            GJAPI.Users.VerifyCallback += LinkAccountCallback;
        }

        private void Start()
        {
            optionsPanel = FindObjectOfType<OptionsPanel>();

            if (!optionsPanel)
            {
                Destroy(gameObject);
                return;
            }

            username.text = optionsPanel.GetGameJoltUsername();
            token.text = optionsPanel.GetGameJoltToken();
        }

        private void LinkAccountCallback(bool isLegit)
        {
            if (isLegit)
            {
                optionsPanel.SetGameJoltInfo(username.text, token.text);

                slideshow.SetSlide(loginFieldsSlide);

                linkError.text = "Verified and saved! Type: " + ActiveData.GameJoltInfo.GetPlayerType(username.text) + ".";
            }
            else
            {
                linkError.text = "Failed to verify!";
                slideshow.SetSlide(loginFieldsSlide);
            }

            GJAPI.Users.VerifyCallback -= LinkAccountCallback;
        }
    }
}
