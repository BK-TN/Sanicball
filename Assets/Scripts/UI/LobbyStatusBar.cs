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
        private RectTransform clientListEntryPrefab = null;

        private List<RectTransform> curClientListEntries = new List<RectTransform>();

        private MatchManager manager;

        private void Start()
        {
            manager = FindObjectOfType<MatchManager>();
            UpdateText();
        }

        private void UpdateText()
        {
            int clients = manager.Clients.Count;
            int players = manager.Players.Count;
            leftText.text = serverName;
            rightText.text = clients + " " + (clients != 1 ? "clients" : "client") + " connected playing with " + players + " " + (players != 1 ? "players" : "player");

            foreach (RectTransform rt in curClientListEntries)
            {
                Destroy(rt.gameObject);
            }
            curClientListEntries.Clear();

            foreach (MatchClient c in manager.Clients)
            {
                RectTransform instance = Instantiate(clientListEntryPrefab);
                instance.SetParent(clientList, false);
                instance.GetComponentInChildren<Text>().text = c.Name;
                curClientListEntries.Add(instance);
            }
        }

        private void Update()
        {
            UpdateText();
        }
    }
}