﻿using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PlayerPortraitLocal : MonoBehaviour
    {
        private const int spacing = 64 + 10;

        private int targetPosition = 0;

        [SerializeField]
        private Text positionField;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text nameField;

        private RectTransform trans;
        public RacePlayerLocal TargetPlayer { get; set; }

        public void Move(int newPosition)
        {
            targetPosition = newPosition;
        }

        // Use this for initialization
        private void Start()
        {
            trans = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (TargetPlayer != null)
            {
                targetPosition = TargetPlayer.RaceFinished ? (TargetPlayer.FinishReport.Position) : (TargetPlayer.Position);
                //Position field
                positionField.text = targetPosition + GetPostfix(targetPosition);
                if (TargetPlayer.RaceFinished) positionField.color = new Color(0f, 0.5f, 1f);
                //Ball icon
                icon.sprite = Data.ActiveData.Characters[TargetPlayer.Character].icon;
                //Name field
				if (TargetPlayer.CtrlType != ControlType.None){
//                    nameField.text = Utils.CtrlTypeStr(TargetPlayer.CtrlType);
					nameField.text = TargetPlayer.Name;

				}else
                    nameField.text = "";
            }

            float y = trans.anchoredPosition.y;

            y = Mathf.Lerp(y, -(targetPosition - 1) * spacing, Time.deltaTime * 10);

            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, y);
        }

        private string GetPostfix(int i)
        {
            if (i % 10 == 1 && i % 100 != 11) return "st";
            if (i % 10 == 2 && i % 100 != 12) return "nd";
            if (i % 10 == 3 && i % 100 != 13) return "rd";
            return "th";
        }
    }
}