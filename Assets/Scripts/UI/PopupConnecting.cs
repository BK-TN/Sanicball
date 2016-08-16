using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PopupConnecting : MonoBehaviour
    {
        [SerializeField]
        private Text titleField = null;
        [SerializeField]
        private Image spinner = null;

        public void Failed(string text)
        {
            titleField.text = text;
            spinner.enabled = false;
        }
    }
}