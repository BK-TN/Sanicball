using System.Collections;
using Lidgren.Network;
using UnityEngine;

namespace Sanicball.Net
{
    public class NetManager : MonoBehaviour
    {
        private NetClient client;
        private NetConnection conn;
        private readonly NetPeerConfiguration conf = new NetPeerConfiguration("Sanicball");
        private MatchManager matchManager;

        public MatchManager MatchManager
        {
            get { return matchManager; }
            set
            {
                if (matchManager == null)
                    matchManager = value;
                else
                    throw new System.InvalidOperationException("Match manager already set");
            }
        }

        public void Start()
        {
            if (matchManager == null)
            {
                throw new System.NullReferenceException("Match manager has not been set");
            }

            DontDestroyOnLoad(gameObject);
        }

        public bool Connect()
        {
            client = new NetClient(conf);
            client.Start();
            conn = client.Connect("127.0.0.1", 25000);
            return false;
        }

        public void OnApplicationQuit()
        {
            client.Disconnect("Closed the game.");
        }

        private void Update()
        {
            if (conn != null)
            {
                //Debug.LogWarning("Conn: " + conn.AverageRoundtripTime);
            }

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                Debug.Log(msg.MessageType);
            }
        }
    }
}