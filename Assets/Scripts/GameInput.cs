using Sanicball.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sanicball
{
    public static class GameInput
    {
        #region Joystick axis names

        private const string joystick1LeftX = "joy1-lx";
        private const string joystick1LeftY = "joy1-ly";
        private const string joystick1RightX = "joy1-rx";
        private const string joystick1RightY = "joy1-ry";
        private const string joystick1DpadX = "joy1-dx";
        private const string joystick1DpadY = "joy1-dy";

        private const string joystick2LeftX = "joy2-lx";
        private const string joystick2LeftY = "joy2-ly";
        private const string joystick2RightX = "joy2-rx";
        private const string joystick2RightY = "joy2-ry";
        private const string joystick2DpadX = "joy2-dx";
        private const string joystick2DpadY = "joy2-dy";

        private const string joystick3LeftX = "joy3-lx";
        private const string joystick3LeftY = "joy3-ly";
        private const string joystick3RightX = "joy3-rx";
        private const string joystick3RightY = "joy3-ry";
        private const string joystick3DpadX = "joy3-dx";
        private const string joystick3DpadY = "joy3-dy";

        private const string joystick4LeftX = "joy4-lx";
        private const string joystick4LeftY = "joy4-ly";
        private const string joystick4RightX = "joy4-rx";
        private const string joystick4RightY = "joy4-ry";
        private const string joystick4DpadX = "joy4-dx";
        private const string joystick4DpadY = "joy4-dy";

        #endregion Joystick axis names

        public static bool KeyboardDisabled { get; set; }

        public static string GetKeyCodeName(KeyCode kc)
        {
            string n = kc.ToString();
            //Insert spaces before capital letters
            for (var i = 0; i < n.Length; i++)
            {
                if (i != 0 && !char.IsLower(n[i]) && char.IsLower(n[i - 1]))
                {
                    n = n.Substring(0, i) + " " + n.Substring(i);
                    //Increment i to account for the newly inserted space
                    i++;
                }
            }

            return n;
        }

        public static string GetControlTypeName(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return "Keyboard";

                case ControlType.Joystick1:
                    return "Joystick #1";

                case ControlType.Joystick2:
                    return "Joystick #2";

                case ControlType.Joystick3:
                    return "Joystick #3";

                case ControlType.Joystick4:
                    return "Joystick #4";
            }
            return null;
        }

        private static Vector2 KeysToVector2(bool right, bool left, bool up, bool down)
        {
            Vector2 output = Vector2.zero;
            if (right && !left)
            {
                output = new Vector3(+1, output.y);
            }
            if (left && !right)
            {
                output = new Vector3(-1, output.y);
            }
            if (up && !down)
            {
                output = new Vector3(output.x, +1);
            }
            if (down && !up)
            {
                output = new Vector3(output.x, -1);
            }
            return output;
        }

        private static Vector3 KeysToVector3(bool right, bool left, bool up, bool down, bool forward, bool back)
        {
            Vector3 output = Vector3.zero;
            if (right && !left)
            {
                output = new Vector3(+1, output.y, output.z);
            }
            if (left && !right)
            {
                output = new Vector3(-1, output.y, output.z);
            }
            if (up && !down)
            {
                output = new Vector3(output.x, +1, output.z);
            }
            if (down && !up)
            {
                output = new Vector3(output.x, -1, output.z);
            }
            if (forward && !back)
            {
                output = new Vector3(output.x, output.y, +1);
            }
            if (back && !forward)
            {
                output = new Vector3(output.x, output.y, -1);
            }
            return output;
        }

        #region Controller specific input

        public static Vector3 MovementVector(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    bool forward = Input.GetKey(ActiveData.Keybinds[Keybind.Forward]) && !KeyboardDisabled;
                    bool left = Input.GetKey(ActiveData.Keybinds[Keybind.Left]) && !KeyboardDisabled;
                    bool back = Input.GetKey(ActiveData.Keybinds[Keybind.Back]) && !KeyboardDisabled;
                    bool right = Input.GetKey(ActiveData.Keybinds[Keybind.Right]) && !KeyboardDisabled;
                    //bool up = Input.GetKey(KeyCode.E) && !KeyboardDisabled;
                    //bool down = Input.GetKey(KeyCode.Q) && !KeyboardDisabled;
                    return KeysToVector3(right, left, false, false, forward, back);

                case ControlType.Joystick1:
                    return new Vector2(Input.GetAxis(joystick1LeftX), Input.GetAxis(joystick1LeftY));

                case ControlType.Joystick2:
                    return new Vector2(Input.GetAxis(joystick2LeftX), Input.GetAxis(joystick2LeftY));

                case ControlType.Joystick3:
                    return new Vector2(Input.GetAxis(joystick3LeftX), Input.GetAxis(joystick3LeftY));

                case ControlType.Joystick4:
                    return new Vector2(Input.GetAxis(joystick4LeftX), Input.GetAxis(joystick4LeftY));
            }
            return Vector2.zero;
        }

        public static Vector2 CameraVector(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    bool up = Input.GetKey(ActiveData.Keybinds[Keybind.CameraUp]) && !KeyboardDisabled;
                    bool left = Input.GetKey(ActiveData.Keybinds[Keybind.CameraLeft]) && !KeyboardDisabled;
                    bool down = Input.GetKey(ActiveData.Keybinds[Keybind.CameraDown]) && !KeyboardDisabled;
                    bool right = Input.GetKey(ActiveData.Keybinds[Keybind.CameraRight]) && !KeyboardDisabled;
                    return KeysToVector2(right, left, up, down);

                case ControlType.Joystick1:
                    return new Vector2(Input.GetAxis(joystick1RightX), Input.GetAxis(joystick1RightY));

                case ControlType.Joystick2:
                    return new Vector2(Input.GetAxis(joystick2RightX), Input.GetAxis(joystick2RightY));

                case ControlType.Joystick3:
                    return new Vector2(Input.GetAxis(joystick3RightX), Input.GetAxis(joystick3RightY));

                case ControlType.Joystick4:
                    return new Vector2(Input.GetAxis(joystick4RightX), Input.GetAxis(joystick4RightY));
            }
            return Vector2.zero;
        }

        public static bool UIUp(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKey(KeyCode.UpArrow);

                case ControlType.Joystick1:
                    return Input.GetAxis(joystick1DpadY) == 1f;
                //return Input.GetKeyDown(KeyCode.Joystick1Button5);
                case ControlType.Joystick2:
                    return Input.GetAxis(joystick2DpadY) == 1f;

                case ControlType.Joystick3:
                    return Input.GetAxis(joystick3DpadY) == 1f;

                case ControlType.Joystick4:
                    return Input.GetAxis(joystick4DpadY) == 1f;
            }
            return false;
        }

        public static bool UIDown(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKey(KeyCode.DownArrow);

                case ControlType.Joystick1:
                    return Input.GetAxis(joystick1DpadY) == -1f;
                //return Input.GetKeyDown(KeyCode.Joystick1Button6);
                case ControlType.Joystick2:
                    return Input.GetAxis(joystick2DpadY) == -1f;

                case ControlType.Joystick3:
                    return Input.GetAxis(joystick3DpadY) == -1f;

                case ControlType.Joystick4:
                    return Input.GetAxis(joystick4DpadY) == -1f;
            }
            return false;
        }

        public static bool UILeft(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKey(KeyCode.LeftArrow);

                case ControlType.Joystick1:
                    return Input.GetAxis(joystick1DpadX) == -1f;
                //return Input.GetKeyDown(KeyCode.Joystick1Button7);
                case ControlType.Joystick2:
                    return Input.GetAxis(joystick2DpadX) == -1f;

                case ControlType.Joystick3:
                    return Input.GetAxis(joystick3DpadX) == -1f;

                case ControlType.Joystick4:
                    return Input.GetAxis(joystick4DpadX) == -1f;
            }
            return false;
        }

        public static bool UIRight(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKey(KeyCode.RightArrow);

                case ControlType.Joystick1:
                    return Input.GetAxis(joystick1DpadX) == 1f;
                //return Input.GetKeyDown(KeyCode.Joystick1Button8);
                case ControlType.Joystick2:
                    return Input.GetAxis(joystick2DpadX) == 1f;

                case ControlType.Joystick3:
                    return Input.GetAxis(joystick3DpadX) == 1f;

                case ControlType.Joystick4:
                    return Input.GetAxis(joystick4DpadX) == 1f;
            }
            return false;
        }

        public static bool IsBraking(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKey(ActiveData.Keybinds[Keybind.Brake]) && !KeyboardDisabled;

                case ControlType.Joystick1:
                    return Input.GetKey(KeyCode.Joystick1Button1);

                case ControlType.Joystick2:
                    return Input.GetKey(KeyCode.Joystick2Button1);

                case ControlType.Joystick3:
                    return Input.GetKey(KeyCode.Joystick3Button1);

                case ControlType.Joystick4:
                    return Input.GetKey(KeyCode.Joystick4Button1);
            }
            return false;
        }

        public static bool IsJumping(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKeyDown(ActiveData.Keybinds[Keybind.Jump]) && !KeyboardDisabled;

                case ControlType.Joystick1:
                    return Input.GetKeyDown(KeyCode.Joystick1Button0);

                case ControlType.Joystick2:
                    return Input.GetKeyDown(KeyCode.Joystick2Button0);

                case ControlType.Joystick3:
                    return Input.GetKeyDown(KeyCode.Joystick3Button0);

                case ControlType.Joystick4:
                    return Input.GetKeyDown(KeyCode.Joystick4Button0);
            }
            return false;
        }

        public static bool IsRespawning(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKeyDown(ActiveData.Keybinds[Keybind.Respawn]) && !KeyboardDisabled;

                case ControlType.Joystick1:
                    return Input.GetKeyDown(KeyCode.Joystick1Button3);

                case ControlType.Joystick2:
                    return Input.GetKeyDown(KeyCode.Joystick2Button3);

                case ControlType.Joystick3:
                    return Input.GetKeyDown(KeyCode.Joystick3Button3);

                case ControlType.Joystick4:
                    return Input.GetKeyDown(KeyCode.Joystick4Button3);
            }
            return false;
        }

        public static bool IsOpeningMenu(ControlType ctrlType)
        {
            switch (ctrlType)
            {
                case ControlType.Keyboard:
                    return Input.GetKeyDown(ActiveData.Keybinds[Keybind.Menu]) && !KeyboardDisabled;

                case ControlType.Joystick1:
                    return Input.GetKeyDown(KeyCode.Joystick1Button2);

                case ControlType.Joystick2:
                    return Input.GetKeyDown(KeyCode.Joystick2Button2);

                case ControlType.Joystick3:
                    return Input.GetKeyDown(KeyCode.Joystick3Button2);

                case ControlType.Joystick4:
                    return Input.GetKeyDown(KeyCode.Joystick4Button2);
            }
            return false;
        }

        #endregion Controller specific input

        #region Keyboard only input

        public static bool IsChangingSong()
        {
            return Input.GetKey(ActiveData.Keybinds[Keybind.NextSong]) && !KeyboardDisabled;
        }

        public static bool IsOpeningChat()
        {
            return Input.GetKey(ActiveData.Keybinds[Keybind.Chat]) && !KeyboardDisabled;
        }

        #endregion Keyboard only input
    }
}