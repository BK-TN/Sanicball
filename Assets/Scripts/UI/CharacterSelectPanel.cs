using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class CharacterSelectionArgs : EventArgs
    {
        public int SelectedCharacter { get; set; }

        public CharacterSelectionArgs(int selectedCharacter)
        {
            SelectedCharacter = selectedCharacter;
        }
    }

    public class CharacterSelectPanel : MonoBehaviour
    {
        [SerializeField]
        private RectTransform entryContainer = null;
        [SerializeField]
        private CharacterSelectEntry entryPrefab = null;
        [SerializeField]
        private Text characterNameLabel;

        [SerializeField]
        private float scrollSpeed = 1f;

        [SerializeField]
        private float normalIconSize = 64;
        [SerializeField]
        private float selectedIconSize = 96;

        private int selected = 0;
        private Data.CharacterInfo selectedChar;
        private float targetX = 0;
        private List<CharacterSelectEntry> activeEntries = new List<CharacterSelectEntry>();

        [SerializeField]
        private Sprite cancelIconSprite;

        public event EventHandler<CharacterSelectionArgs> CharacterSelected;
        public event EventHandler CancelSelected;

        private IEnumerator Start()
        {
            var charList = ActiveData.Characters.OrderBy(a => a.hyperspeed).ToArray();

            CharacterSelectEntry cancelEnt = Instantiate(entryPrefab);
            cancelEnt.IconImage.sprite = cancelIconSprite;
            cancelEnt.transform.SetParent(entryContainer.transform, false);
            activeEntries.Add(cancelEnt);

            for (int i = 0; i < charList.Length; i++)
            {
                if (!charList[i].hidden)
                {
                    CharacterSelectEntry characterEnt = Instantiate(entryPrefab);

                    characterEnt.Init(charList[i]);
                    characterEnt.transform.SetParent(entryContainer.transform, false);
                    activeEntries.Add(characterEnt);
                }
            }

            //Wait a single frame before selecting the first character.
            yield return null;
            Select(1);
        }

        public void NextCharacter()
        {
            if (selected < activeEntries.Count - 1) Select(selected + 1); else Select(0);
        }

        public void PrevCharacter()
        {
            if (selected > 0) Select(selected - 1); else Select(activeEntries.Count - 1);
        }

        private void Select(int newSelection)
        {
            selected = newSelection;
            selectedChar = activeEntries[selected].Character;

            if (selected == 0)
                characterNameLabel.text = "Leave match";
            else
                characterNameLabel.text = selectedChar.name;
        }

        private void Update()
        {
            //Find the container's target X to center the selected character
            targetX = entryContainer.sizeDelta.x / 2 - activeEntries[selected].RectTransform.anchoredPosition.x;
            if (!Mathf.Approximately(entryContainer.anchoredPosition.x, targetX))
            {
                float x = Mathf.Lerp(entryContainer.anchoredPosition.x, targetX, scrollSpeed * Time.deltaTime);

                entryContainer.anchoredPosition = new Vector2(x, entryContainer.anchoredPosition.y);
            }

            //Resize all elements
            for (int i = 0; i < activeEntries.Count; i++)
            {
                var element = activeEntries[i];

                float targetSize = (i == selected) ? selectedIconSize : normalIconSize;

                if (!Mathf.Approximately(element.Size, targetSize))
                {
                    element.Size = Mathf.Lerp(element.Size, targetSize, scrollSpeed * Time.deltaTime);
                }
            }
        }

        public void Accept()
        {
            if (selected == 0)
            {
                if (CancelSelected != null)
                    CancelSelected(this, EventArgs.Empty);
            }
            else
            {
                if (CharacterSelected != null)
                    CharacterSelected(this, new CharacterSelectionArgs(Array.IndexOf(ActiveData.Characters, selectedChar)));
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}