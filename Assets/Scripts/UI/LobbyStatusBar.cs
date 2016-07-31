using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class LobbyStatusBar : MonoBehaviour
    {
        public string serverName;
        public int players;
        public int spectators;

        public Text leftText;
        public Text rightText;

        private void Update()
        {
            leftText.text = serverName;
            rightText.text = players + " " + (players != 1 ? "players" : "player");
            if (spectators > 0)
            {
                rightText.text += " and " + spectators + " " + (spectators != 1 ? "spectators" : "spectator");
            }
        }
    }
}
