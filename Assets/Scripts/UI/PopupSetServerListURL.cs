using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Popup))]
    public class PopupSetServerListURL : MonoBehaviour
    {
        public InputField url;

        private OptionsPanel optionsPanel;

        public void Validate()
        {
			string u = url.text.Trim();
			optionsPanel.SetServerListURL(u);
            GetComponent<Popup>().Close();
        }

        private void Start()
        {
            optionsPanel = FindObjectOfType<OptionsPanel>();

            if (!optionsPanel)
            {
                Destroy(gameObject);
                return;
            }

			url.text = optionsPanel.GetServerListURL() ?? "";
        }
    }
}