using System.Collections;
using UnityEngine;

namespace Sanicball
{
    public static class Utils
    {
        public static string CtrlTypeStr(ControlType ctrlType)
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
            return "None";
        }
    }
}