using System.Collections.Generic;
using Sanicball.Data;
using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class LobbyStatusBar : MonoBehaviour
    {
        [SerializeField]
        private Text leftText = null;
        [SerializeField]
        private Text rightText = null;

        [SerializeField]
        private RectTransform clientList = null;
        [SerializeField]
        private ClientListEntry clientListEntryPrefab = null;

        private List<ClientListEntry> curClientListEntries = new List<ClientListEntry>();

        private MatchManager manager;

        private void Start()
        {
            manager = FindObjectOfType<MatchManager>();

            //Self destruct if not in online mode
            if (!manager.OnlineMode)
            {
                Destroy(gameObject);
                return;
            }
            UpdateText();
        }

        private void UpdateText()
        {
            if (!manager) return;

            int clients = manager.Clients.Count;
            int players = manager.Players.Count;

            if (manager.AutoStartTimerOn)
            {
                leftText.text = "Match will start in " + GetTimeString(System.TimeSpan.FromSeconds(manager.AutoStartTimer)) + ", or when all players are ready.";
            }
            else if (manager.Players.Count > 0)
            {
                leftText.text = "Match starts when all players are ready.";
            }
            else
            {
                leftText.text = "Match will not start without players.";
            }
            rightText.text = clients + " " + (clients != 1 ? "clients" : "client") + " - " + players + " " + (players != 1 ? "players" : "player");

            foreach (ClientListEntry entry in curClientListEntries)
            {
                Destroy(entry.gameObject);
            }
            curClientListEntries.Clear();

            foreach (MatchClient c in manager.Clients)
            {
                ClientListEntry listEntry = Instantiate(clientListEntryPrefab);
                listEntry.transform.SetParent(clientList, false);

                listEntry.FillFields(c, manager);
                curClientListEntries.Add(listEntry);
            }
        }

        private void Update()
        {
            UpdateText();
        }

        private string GetTimeString(System.TimeSpan timeToUse)
        {
            return string.Format("{0:00}:{1:00}", timeToUse.Minutes, timeToUse.Seconds);
        }
    }
}