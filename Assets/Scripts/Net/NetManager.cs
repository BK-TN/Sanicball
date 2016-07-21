using System.Collections;
using Lidgren.Network;
using UnityEngine;

namespace Sanicball.Net
{
    public class NetManager : MonoBehaviour
    {
        public const string APP_ID = "Sanicball";

        private NetClient client;
        private NetConnection conn;
        private MatchManager matchManager;

        [SerializeField]
        private MatchManager matchManagerPrefab = null;

        public void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnApplicationQuit()
        {
            client.Disconnect("Closed the game.");
        }

        private void Update()
        {
        }

        private void MatchManager_SettingsChangeRequested(object sender, SettingsChangeArgs e)
        {
            NetOutgoingMessage settingsMsg = client.CreateMessage();
            settingsMsg.Write(MessageType.MatchSettingsChanged);
            string serializedSettings = Newtonsoft.Json.JsonConvert.SerializeObject(e.NewSettings);
            Debug.Log(serializedSettings);
            settingsMsg.Write(serializedSettings);
            conn.SendMessage(settingsMsg, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }
}