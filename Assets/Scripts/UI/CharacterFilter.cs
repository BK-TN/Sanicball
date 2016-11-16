using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class CharacterFilter : MonoBehaviour
    {
        [SerializeField]
        private CharacterButton buttonPrefab;
        [SerializeField]
        private RectTransform buttonContainer;

        [SerializeField]
        private Selectable above;
        [SerializeField]
        private Selectable below;

        private bool[] filter;
        private int latestSelect;

        private void Start()
        {
            filter = new bool[Data.ActiveData.Characters.Length];

            List<CharacterButton> buttons = new List<CharacterButton>();

            for (int i = 0; i < Data.ActiveData.Characters.Length; i++)
            {
                Data.CharacterInfo c = Data.ActiveData.Characters[i];
                int i2 = i;

                if (c.hidden) continue;

                CharacterButton button = Instantiate(buttonPrefab);
                buttons.Add(button);
                button.Mark(true);
                button.transform.SetParent(buttonContainer, false);
                button.ButtonComponent.onClick.AddListener(() =>
                {
                    filter[i2] = !filter[i2];
                    button.Mark(!filter[i2]);
                });
                button.Init(c);
            }

            SetupNavigation(buttons);
        }

        private void SetupNavigation(List<CharacterButton> buttons)
        {
            Navigation nav = above.navigation;
            nav.selectOnDown = buttons[0].GetComponent<Button>();
            above.navigation = nav;

            nav = below.navigation;
            nav.selectOnUp = buttons[0].GetComponent<Button>();
            below.navigation = nav;

            for (int i = 0; i < buttons.Count; i++)
            {
                CharacterButton button = buttons[i];

                Navigation buttonNav = button.ButtonComponent.navigation;
                buttonNav.mode = Navigation.Mode.Explicit;
                buttonNav.selectOnUp = above;
                buttonNav.selectOnDown = below;

                if (i != 0)
                {
                    buttonNav.selectOnLeft = buttons[i - 1].ButtonComponent;
                }

                if (i != buttons.Count - 1)
                {
                    buttonNav.selectOnRight = buttons[i + 1].ButtonComponent;
                }

                button.ButtonComponent.navigation = buttonNav;
            }
        }

        private void Update()
        {
        }
    }
}