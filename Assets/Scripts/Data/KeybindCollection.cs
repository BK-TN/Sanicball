using System.Collections.Generic;
using UnityEngine;

namespace Sanicball.Data
{
    //TODO: use this enum for keybinds
    public enum Keybind
    {
        Forward,
        Left,
        Back,
        Right,

        CameraUp,
        CameraLeft,
        CameraDown,
        CameraRight,

        Brake,
        Jump,
        Respawn, //Toggle ready in lobby, respawn ingame
        Menu,
        NextSong,
        Chat
    }

    [System.Serializable]
    public class KeybindCollection
    {
        public Dictionary<Keybind, KeyCode> keybinds = new Dictionary<Keybind, KeyCode>();

        /// <summary>
        /// Creates an instance and assigns default keybinds.
        /// </summary>
        public KeybindCollection()
        {
            //Set default keybinds
            keybinds.Add(Keybind.Forward, KeyCode.W);
            keybinds.Add(Keybind.Left, KeyCode.A);
            keybinds.Add(Keybind.Back, KeyCode.S);
            keybinds.Add(Keybind.Right, KeyCode.D);

            keybinds.Add(Keybind.CameraUp, KeyCode.UpArrow);
            keybinds.Add(Keybind.CameraLeft, KeyCode.LeftArrow);
            keybinds.Add(Keybind.CameraDown, KeyCode.DownArrow);
            keybinds.Add(Keybind.CameraRight, KeyCode.RightArrow);

            keybinds.Add(Keybind.Brake, KeyCode.LeftShift);
            keybinds.Add(Keybind.Jump, KeyCode.Space);
            keybinds.Add(Keybind.Respawn, KeyCode.R);
            keybinds.Add(Keybind.Menu, KeyCode.Return);
            keybinds.Add(Keybind.NextSong, KeyCode.N);
            keybinds.Add(Keybind.Chat, KeyCode.T);

            //Joystick 1
            /*keybinds.Add("joy1brake", KeyCode.Joystick1Button1); //X / A
            keybinds.Add("joy1jump", KeyCode.Joystick1Button0); //O / B
            keybinds.Add("joy1respawn", KeyCode.Joystick1Button3); //Triangle / Y
            keybinds.Add("joy1menu", KeyCode.Joystick1Button2); //Square / X*/
        }

        public KeyCode this[Keybind b]
        {
            get
            {
                return keybinds[b];
            }

            set
            {
                keybinds[b] = value;
            }
        }

        /// <summary>
        /// Copies all values from the supplied collection.
        /// </summary>
        /// <param name="original"></param>
        public void CopyValues(KeybindCollection original)
        {
            keybinds = new Dictionary<Keybind, KeyCode>(original.keybinds);
        }
    }
}