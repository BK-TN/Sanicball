using UnityEngine;

namespace Sanicball.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        private SlideCanvasGroup activePanel;

        public void SetActivePanel(SlideCanvasGroup panel)
        {
            if (activePanel == null)
            {
                //Opening new panel
                panel.Open();
                activePanel = panel;
            }
            else
            {
                //Changing panel
                if (activePanel != panel)
                {
                    CloseActivePanel();
                    panel.Open();
                    activePanel = panel;
                }
                else
                {
                    //Closing active panel
                    CloseActivePanel();
                }
            }
        }

        public void CloseActivePanel()
        {
            activePanel.Close();
            activePanel = null;
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) && FindObjectsOfType<Popup>().Length <= 0)
            {
                if (activePanel != null)
                {
                    CloseActivePanel();
                }
                else
                {
                    Application.Quit();
                }
            }
        }
    }
}