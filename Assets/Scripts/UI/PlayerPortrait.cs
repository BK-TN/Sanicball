using Sanicball.Data;
using Sanicball.Logic;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PlayerPortrait : MonoBehaviour
    {
        private const int spacing = 64;

        private int targetPosition = 0;

        [SerializeField]
        private Text positionField;

        [SerializeField]
        private Image characterImage;

        [SerializeField]
        private Text nameField;

        private RectTransform trans;
        public RacePlayer TargetPlayer { get; set; }

        public void Move(int newPosition)
        {
            targetPosition = newPosition;
        }

        // Use this for initialization
        private void Start()
        {
            trans = GetComponent<RectTransform>();

            TargetPlayer.Destroyed += DestroyedCallback;
        }

        private void DestroyedCallback(object sender, System.EventArgs e)
        {
            if (this)
                Destroy(gameObject);
            TargetPlayer.Destroyed -= DestroyedCallback;
        }

        // Update is called once per frame
        private void Update()
        {
            targetPosition = TargetPlayer.RaceFinished ? (TargetPlayer.FinishReport.Position) : (TargetPlayer.Position);
            //Position field
            positionField.text = Utils.GetPosString(targetPosition);
            if (TargetPlayer.RaceFinished) positionField.color = new Color(0f, 0.5f, 1f);
            //Image representing character
            characterImage.color = ActiveData.Characters[TargetPlayer.Character].color;
            //Name field
            nameField.text = TargetPlayer.Name;

            float y = trans.anchoredPosition.y;

            y = Mathf.Lerp(y, -(targetPosition - 1) * spacing, Time.deltaTime * 10);

            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, y);
        }
    }
}