using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ControlsPanel : MonoBehaviour
    {
        [Header("Fields")]
        public Text forward;

        public Text left;
        public Text back;
        public Text right;
        public Text cameraUp;
        public Text cameraLeft;
        public Text cameraDown;
        public Text cameraRight;
        public Text brake;
        public Text jump;
        public Text respawn;
        public Text nextSong;
        public Text menu;
        public Text chat;
        private Keybind keybindToChange;

        private KeybindCollection tempKeybinds = new KeybindCollection();

        public void Apply()
        {
            ActiveData.Keybinds.CopyValues(tempKeybinds);
        }

        public void RevertToCurrent()
        {
            tempKeybinds.CopyValues(ActiveData.Keybinds);
            UpdateFields();
        }

        public void ResetToDefault()
        {
            tempKeybinds = new KeybindCollection();
            UpdateFields();
        }

        public void UpdateFields()
        {
            forward.text = Str(Keybind.Forward);
            left.text = Str(Keybind.Left);
            back.text = Str(Keybind.Back);
            right.text = Str(Keybind.Right);

            cameraUp.text = Str(Keybind.CameraUp);
            cameraLeft.text = Str(Keybind.CameraLeft);
            cameraDown.text = Str(Keybind.CameraDown);
            cameraRight.text = Str(Keybind.CameraRight);

            brake.text = Str(Keybind.Brake);
            jump.text = Str(Keybind.Jump);
            respawn.text = Str(Keybind.Respawn);
            nextSong.text = Str(Keybind.NextSong);
            menu.text = Str(Keybind.Menu);
            chat.text = Str(Keybind.Chat);
        }

        public void SetKeybindToChange(string name)
        {
            keybindToChange = (Keybind)System.Enum.Parse(typeof(Keybind), name, true);
        }

        public void ChangeKeybind(KeyCode key)
        {
            //if (!string.IsNullOrEmpty(keybindToChange))
            //{
            tempKeybinds[keybindToChange] = key;
            //}
            UpdateFields();
        }

        private void Start()
        {
            RevertToCurrent();
        }

        private string Str(Keybind keybind)
        {
            return GameInput.GetKeyCodeName(tempKeybinds[keybind]);
        }
    }
}
