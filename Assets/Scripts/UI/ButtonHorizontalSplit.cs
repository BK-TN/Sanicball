using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ButtonHorizontalSplit : MonoBehaviour, IPointerClickHandler, IMoveHandler
    {
        public RectTransform splitPosition;

        public UnityEvent onClickLeft;
        public UnityEvent onClickRight;

        public void OnPointerClick(PointerEventData data)
        {
            float pos = data.position.x;

            Vector3[] corners = new Vector3[4];
            splitPosition.GetWorldCorners(corners);

            var topLeftX = corners[0].x;
            var topRightX = corners[3].x;

            var middleX = Mathf.Lerp(topLeftX, topRightX, 0.5f);

            if (pos > middleX)
            {
                onClickRight.Invoke();
            }
            else {
                onClickLeft.Invoke();
            }
        }

        public void OnMove(AxisEventData data)
        {
            MoveDirection moveDir = data.moveDir;
            if (moveDir == MoveDirection.Left)
            {
                onClickLeft.Invoke();
                data.Use();
            }
            if (moveDir == MoveDirection.Right)
            {
                onClickRight.Invoke();
                data.Use();
            }
            //Invoke button's onclick if it's there
            var button = GetComponent<Button>();
            if ((moveDir == MoveDirection.Left || moveDir == MoveDirection.Right) && button)
            {
                button.onClick.Invoke();
            }
        }
    }
}
