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
            NetOutgoingMessage approval = client.CreateMessage();
            approval.Write("Approve me please");
            conn = client.Connect("127.0.0.1", 25000, approval);
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
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Debug.Log(msg.ReadString());
                        break;

                    case NetIncomingMessageType.WarningMessage:
                        Debug.LogWarning(msg.ReadString());
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        Debug.LogError(msg.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        byte status = msg.ReadByte();
                        string statusMsg = msg.ReadString();
                        Debug.Log("Status change recieved: " + (NetConnectionStatus)status + " - Message: " + statusMsg);
                        break;

                    default:
                        Debug.Log("Recieved unhandled message of type " + msg.MessageType);
                        break;
                }
            }
        }
    }
}