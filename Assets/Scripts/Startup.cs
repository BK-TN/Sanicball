using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball
{
    public class Startup : MonoBehaviour
    {
        public UI.Intro intro;
        public CanvasGroup setNicknameGroup;
        public InputField nicknameField;

        [SerializeField]
        private bool skipNickname = true;

        public void ValidateNickname()
        {
            if (nicknameField.text.Trim() != "")
            {
                setNicknameGroup.alpha = 0f;
                ActiveData.GameSettings.nickname = nicknameField.text;
                intro.enabled = true;
            }
        }

        private void Start()
        {
            if (skipNickname)
            {
                ActiveData.GameSettings.nickname = "Player";
            }
            if (string.IsNullOrEmpty(ActiveData.GameSettings.nickname))
            {
                //Set nickname before continuing
                setNicknameGroup.alpha = 1f;
            }
            else
            {
                setNicknameGroup.alpha = 0f;
                intro.enabled = true;
            }
        }
    }
}