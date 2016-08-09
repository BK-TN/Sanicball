using System.Collections.Generic;
using Sanicball.Match;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class LobbyStatusBar : MonoBehaviour
    {
        public string serverName;

        public Text leftText;
        public Text rightText;

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
            int clients = manager.Clients.Count;
            int players = manager.Players.Count;
            leftText.text = serverName;
            rightText.text = clients + " " + (clients != 1 ? "clients" : "client") + " connected playing with " + players + " " + (players != 1 ? "players" : "player");

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
    }
}