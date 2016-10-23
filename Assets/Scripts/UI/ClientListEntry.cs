using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ClientListEntry : MonoBehaviour
    {
        [SerializeField]
        private Text nameField = null;
        [SerializeField]
        private Text playerCountField = null;

        public void FillFields(MatchClient client, MatchManager manager)
        {
            nameField.text = client.Name;

            List<MatchPlayer> players = manager.Players.Where(a => a.ClientGuid == client.Guid).ToList();
            int playersTotal = players.Count();
            int playersReady = players.Count(a => a.ReadyToRace);

            if (playersTotal == 0)
            {
                playerCountField.text = "Spectating";
            }
            else
            {
                playerCountField.text = playersReady + "/" + playersTotal + " ready";
            }
        }
    }
}