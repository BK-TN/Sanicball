using System.Linq;
using UnityEngine;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Popup))]
    public class PopupChangeKeybind : MonoBehaviour
    {
        private ControlsPanel panel;

        private KeyCode[] validKeyCodes;

        private void Start()
        {
            panel = FindObjectOfType<ControlsPanel>();
            if (!panel)
            {
                GetComponent<Popup>().Close();
            }
            validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

            //Filter out keycodes with "Mouse" or "Joystick" in their name
            validKeyCodes = validKeyCodes.Where(a => !a.ToString().Contains("Mouse") && !a.ToString().Contains("Joystick")).ToArray();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //panel.SetKeybindToChange(string.Empty);
                GetComponent<Popup>().Close();
                return;
            }
            foreach (KeyCode kc in validKeyCodes)
            {
                if (Input.GetKeyDown(kc) && kc != KeyCode.Escape)
                {
                    panel.ChangeKeybind(kc);
                    //panel.SetKeybindToChange(string.Empty);
                    GetComponent<Popup>().Close();
                    return;
                }
            }
        }
    }
}
