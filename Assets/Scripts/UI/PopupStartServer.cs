using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    //TODO: Make this entire script work with UNET
    public class PopupStartServer : MonoBehaviour
    {
        public InputField serverNameInput;
        public Text serverNameOutput;

        public InputField portInput;
        public Text portOutput;

        public Text maxPlayersText;

        public Text showOnServerListText;
        public Text enableNatPunchingText;

        public CanvasRenderer enableNatPunchingPanel;

        public Slideshow slideshow;

        private int maxPlayers = 12;
        private bool showOnList = true;
        private bool useNat = false;

        public void MaxPlayersUp()
        {
            if (maxPlayers < 64)
                maxPlayers++;
            else
                maxPlayers = 1;

            UpdateFields();
        }

        public void MaxPlayersDown()
        {
            if (maxPlayers > 1)
                maxPlayers--;
            else
                maxPlayers = 64;

            UpdateFields();
        }

        public void ToggleShowOnList()
        {
            showOnList = !showOnList;
            showOnServerListText.text = showOnList ? "Yes" : "No";
            enableNatPunchingPanel.gameObject.SetActive(showOnList);
        }

        public void ToggleNatPunching()
        {
            useNat = !useNat;
            enableNatPunchingText.text = useNat ? "Yes" : "No";
        }

        public void ValidateServerName()
        {
            serverNameOutput.text = "";

            var s = serverNameInput.text;

            bool valid = true;

            if (s.Length <= 0)
            {
                serverNameOutput.text = "Server name can't be empty!";
                valid = false;
            }

            if (valid)
            {
                slideshow.NextSlide();
            }
        }

        public void ValidatePort()
        {
            portOutput.text = "";

            int port;
            if (int.TryParse(portInput.text, out port))
            {
                if (port > 0 && port < 49152)
                {
                    //Success, start the server

                    /*string error;
                    if (serverStarter.StartServer(serverNameInput.text, maxPlayers, port, useNat, showOnList, out error))
                    {
                        Debug.Log("Success!!");
                    }
                    else
                    {
                        portOutput.text = error;
                    }*/
                }
                else
                {
                    portOutput.text = "Port number must be between 1 and 49151.";
                }
            }
            else
            {
                portOutput.text = "Failed to parse port as an integer!";
            }
        }

        private void Start()
        {
            UpdateFields();
            showOnServerListText.text = "Yes";
            enableNatPunchingText.text = "No";
        }

        private void UpdateFields()
        {
            maxPlayersText.text = maxPlayers.ToString();
        }
    }
}