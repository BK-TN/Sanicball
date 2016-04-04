using System;
using System.Collections;
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
        public RectTransform characterIconContainer;
        public RectTransform characterIconPrefab;
        public Text characterNameLabel;

        public float scrollSpeed = 1f;

        public float normalIconSize = 64;
        public float selectedIconSize = 96;
        private int selected = 0;

        private float targetX = 0;
        private RectTransform[] ballIcons;

        [SerializeField]
        private Sprite cancelIconSprite;

        public event EventHandler<CharacterSelectionArgs> CharacterSelected;
        public event EventHandler CancelSelected;

        private IEnumerator Start()
        {
            var charList = Data.ActiveData.Characters;

            ballIcons = new RectTransform[charList.Length + 1];

            ballIcons[0] = Instantiate(characterIconPrefab);
            ballIcons[0].GetComponent<Image>().sprite = cancelIconSprite;
            ballIcons[0].transform.SetParent(characterIconContainer.transform, false);

            for (int i = 0; i < charList.Length; i++)
            {
				if(!charList[i].hyperspeed)
				{
					RectTransform ballIcon = Instantiate(characterIconPrefab);

					ballIcon.GetComponent<Image>().sprite = charList[i].icon;
					ballIcon.transform.SetParent(characterIconContainer.transform, false);
					ballIcons[i + 1] = ballIcon;
				}
            }
			for (int i = 0; i < charList.Length; i++)
			{
				if(charList[i].hyperspeed)
				{
					RectTransform ballIcon = Instantiate(characterIconPrefab);

					ballIcon.GetComponent<Image>().sprite = charList[i].icon;
					ballIcon.transform.SetParent(characterIconContainer.transform, false);
					ballIcons[i + 1] = ballIcon;
				}
			}

            //Wait a single frame before selecting the first character.
            yield return null;
            Select(1);
        }

        public void NextCharacter()
        {
			if (selected == 13)
				Select (15);
			else if (selected == Data.ActiveData.Characters.Length)
				Select (14);
			else if (selected == 14)
				Select (selected);
			else if (selected < ballIcons.Length - 1 && (selected != 13 || selected != Data.ActiveData.Characters.Length))
				Select (selected + 1);
        }

        public void PrevCharacter()
        {
			if (selected == 15)
				Select (13);
			else if (selected == 14)
				Select (Data.ActiveData.Characters.Length);
			else if (selected > 0) 
				Select(selected - 1);
			
        }

        private void Select(int newSelection)
        {
            if (newSelection == 0)
                characterNameLabel.text = "Leave match";
            else
				characterNameLabel.text = Data.ActiveData.Characters[newSelection - 1].name /*+ " (ID: " + newSelection.ToString() + ")"*/;

            selected = newSelection;
        }

        private void Update()
        {
            //Find the container's target X to center the selected character
            targetX = characterIconContainer.sizeDelta.x / 2 - ballIcons[selected].anchoredPosition.x;
            if (!Mathf.Approximately(characterIconContainer.anchoredPosition.x, targetX))
            {
                float x = Mathf.Lerp(characterIconContainer.anchoredPosition.x, targetX, scrollSpeed * Time.deltaTime);

                characterIconContainer.anchoredPosition = new Vector2(x, characterIconContainer.anchoredPosition.y);
            }

            //Make selected element bigger
            var selectedElement = ballIcons[selected].GetComponent<LayoutElement>();
            if (!Mathf.Approximately(selectedElement.preferredWidth, selectedIconSize) || !Mathf.Approximately(selectedElement.preferredHeight, selectedIconSize))
            {
                selectedElement.preferredWidth = Mathf.Lerp(selectedElement.preferredWidth, selectedIconSize, scrollSpeed * Time.deltaTime);
                selectedElement.preferredHeight = Mathf.Lerp(selectedElement.preferredHeight, selectedIconSize, scrollSpeed * Time.deltaTime);
            }

            //Make all other elements normal sized
            for (int i = 0; i < ballIcons.Length; i++)
            {
                if (i == selected) continue;
                var element = ballIcons[i].GetComponent<LayoutElement>();

                if (!Mathf.Approximately(element.preferredWidth, normalIconSize) || !Mathf.Approximately(element.preferredHeight, normalIconSize))
                {
                    element.preferredWidth = Mathf.Lerp(element.preferredWidth, normalIconSize, scrollSpeed * Time.deltaTime);
                    element.preferredHeight = Mathf.Lerp(element.preferredHeight, normalIconSize, scrollSpeed * Time.deltaTime);
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
                    CharacterSelected(this, new CharacterSelectionArgs(selected - 1));
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