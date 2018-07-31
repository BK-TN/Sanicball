using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Popup))]
    public class PopupSetNickname : MonoBehaviour
    {
        public InputField nickname;
        public Text errorOutput;

        private OptionsPanel optionsPanel;

        public void Validate()
        {
            errorOutput.text = "";
            if (string.IsNullOrEmpty(nickname.text.Trim()))
            {
                errorOutput.text = "Nickname can't be empty!";
            }
            else
            {
                string nick = nickname.text.Trim();
                optionsPanel.SetNickname(nick);
                GetComponent<Popup>().Close();
            }
        }

        private void Start()
        {
            optionsPanel = FindObjectOfType<OptionsPanel>();

            if (!optionsPanel)
            {
                Destroy(gameObject);
                return;
            }

            nickname.text = optionsPanel.GetNickname() ?? "";
        }
    }
}