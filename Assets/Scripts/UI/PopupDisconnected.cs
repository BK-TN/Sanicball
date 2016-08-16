using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PopupDisconnected : MonoBehaviour
    {
        [SerializeField]
        private Text reasonField = null;

        public string Reason { set { reasonField.text = value; } }
    }
}