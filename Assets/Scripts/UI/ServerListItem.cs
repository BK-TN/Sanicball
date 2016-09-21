using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ServerListItem : MonoBehaviour
    {
        [SerializeField]
        private Text serverNameText = null;
        [SerializeField]
        private Text serverStatusText = null;
        [SerializeField]
        private Text playerCountText = null;
        [SerializeField]
        private Text pingText = null;

        private ServerInfo info;
        private System.Net.IPEndPoint endpoint;

        public void Init(ServerInfo info, System.Net.IPEndPoint endpoint, int pingMs, bool isLocal)
        {
            serverNameText.text = info.Config.ServerName;
            serverStatusText.text = info.InRace ? "In race" : "In lobby";
            if (isLocal)
            {
                serverStatusText.text += " - LAN server";
            }
            playerCountText.text = info.Players + "/" + info.Config.MaxPlayers;
            pingText.text = pingMs + "ms";

            this.endpoint = endpoint;
        }

        public void Join()
        {
            MatchStarter starter = FindObjectOfType<MatchStarter>();
            if (starter)
            {
                starter.JoinOnlineGame(endpoint);
            }
            else
            {
                Debug.LogError("No match starter found");
            }
        }
    }
}
