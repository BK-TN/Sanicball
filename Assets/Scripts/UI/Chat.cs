using System.Collections;
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

    public class Chat : MonoBehaviour
    {
        [SerializeField]
        private Text chatMessagePrefab = null;

        [SerializeField]
        private RectTransform chatMessageContainer = null;

        [SerializeField]
        private InputField messageInputField = null;

        private GameObject prevSelectedObject;
        private bool shouldEnableInput = false;

        public event System.EventHandler<ChatMessageArgs> MessageSent;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
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

            if (es.currentSelectedGameObject == messageInputField.gameObject && Input.GetKeyDown(KeyCode.Return))
            {
                SendMessage();
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

        public void ShowMessage(string from, string text)
        {
            Text messageObj = Instantiate(chatMessagePrefab);

            messageObj.transform.SetParent(chatMessageContainer, false);
            messageObj.text = string.Format("<color=#0000ff><b>{0}</b></color>: {1}", from, text);
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