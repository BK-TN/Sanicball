using System.Collections;
using System.Linq;
using SanicballCore.MatchMessages;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ChatMessageArgs : System.EventArgs
    {
        public string Text { get; private set; }

        public ChatMessageArgs(string text)
        {
            Text = text;
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class Chat : MonoBehaviour
    {
        private const float MAX_VISIBLE_TIME = 4f;
        private const float FADE_TIME = 0.2f;

        [SerializeField]
        private Text chatMessagePrefab = null;

        [SerializeField]
        private RectTransform chatMessageContainer = null;

        [SerializeField]
        private InputField messageInputField = null;

        [SerializeField]
        private RectTransform hoverArea = null;

        private GameObject prevSelectedObject;
        private bool shouldEnableInput = false;
        private CanvasGroup canvasGroup;
        private float visibleTime = 0;

        public event System.EventHandler<ChatMessageArgs> MessageSent;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        public void Update()
        {
            EventSystem es = EventSystem.current;
            if (GameInput.IsOpeningChat())
            {
                if (es.currentSelectedGameObject != messageInputField.gameObject)
                {
                    prevSelectedObject = es.currentSelectedGameObject;
                    es.SetSelectedGameObject(messageInputField.gameObject);
                }
            }

            if (es.currentSelectedGameObject == messageInputField.gameObject)
            {
                visibleTime = MAX_VISIBLE_TIME;
                if (Input.GetKeyDown(KeyCode.Return))
                    SendMessage();
            }

            if (Input.mousePosition.x < hoverArea.sizeDelta.x && Input.mousePosition.y < hoverArea.sizeDelta.y)
            {
                visibleTime = MAX_VISIBLE_TIME;
            }

            if (visibleTime > 0)
            {
                visibleTime -= Time.deltaTime;
                canvasGroup.alpha = 1;
            }
            else if (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = Mathf.Max(canvasGroup.alpha - Time.deltaTime / FADE_TIME, 0);
            }
        }

        public void LateUpdate()
        {
            if (shouldEnableInput)
            {
                GameInput.KeyboardDisabled = false;
                shouldEnableInput = false;
            }
        }

        public void ShowMessage(ChatMessageType type, string from, string text)
        {
            Text messageObj = Instantiate(chatMessagePrefab);

            messageObj.transform.SetParent(chatMessageContainer, false);
            switch (type)
            {
                case ChatMessageType.User:
                    messageObj.text = string.Format("<color=#6688ff><b>{0}</b></color>: {1}", from, text);
                    break;

                case ChatMessageType.System:
                    messageObj.text = string.Format("<color=#ffff77><b>{0}</b></color>", text);
                    break;
            }

            visibleTime = MAX_VISIBLE_TIME;
        }

        public void DisableInput()
        {
            GameInput.KeyboardDisabled = true;
        }

        public void EnableInput()
        {
            shouldEnableInput = true;
        }

        public void SendMessage()
        {
            string text = messageInputField.text;

            if (text.Trim() != string.Empty)
            {
                if (MessageSent != null)
                    MessageSent(this, new ChatMessageArgs(text));
            }
            EventSystem.current.SetSelectedGameObject(prevSelectedObject);

            messageInputField.text = string.Empty;
        }
    }
}