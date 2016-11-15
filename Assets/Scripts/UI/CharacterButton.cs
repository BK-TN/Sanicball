using System.Collections;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Button))]
    public class CharacterButton : MonoBehaviour
    {
        [SerializeField]
        private Image characterImage;

        private Button buttonComp;

        public UnityEngine.Events.UnityEvent OnClick { get { return buttonComp.onClick; } }

        private void Awake()
        {
            buttonComp = GetComponent<Button>();
        }

        public void Init(Data.CharacterInfo character)
        {
            characterImage.sprite = character.icon;
        }

        public void Mark(bool on)
        {
            characterImage.color = on ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);
        }
    }
}