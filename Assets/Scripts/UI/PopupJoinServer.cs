using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PopupJoinServer : MonoBehaviour
    {
        [SerializeField]
        private InputField ipInput;
        [SerializeField]
        private InputField portInput;
        [SerializeField]
        private Text portOutput;

        private const int LOWEST_PORT_NUM = 1024;
        private const int HIGHEST_PORT_NUM = 49151;

        public void Connect()
        {
            portOutput.text = "";

            int port;
            if (int.TryParse(portInput.text, out port))
            {
                if (port >= LOWEST_PORT_NUM && port <= HIGHEST_PORT_NUM)
                {
                    //Success, start the server
                    MatchStarter matchStarter = FindObjectOfType<MatchStarter>();
                    matchStarter.JoinOnlineGame(ipInput.text, port);
                }
                else
                {
                    portOutput.text = "Port number must be between " + LOWEST_PORT_NUM + " and " + HIGHEST_PORT_NUM + ".";
                }
            }
            else
            {
                portOutput.text = "Port must be a number!";
            }
        }
    }
}
