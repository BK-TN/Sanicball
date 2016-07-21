using Lidgren.Network;
using Sanicball.Net;
using UnityEngine;

namespace Sanicball
{
    public class MatchStarter : MonoBehaviour
    {
        public const string APP_ID = "Sanicball";

        [SerializeField]
        private MatchManager matchManagerPrefab = null;

        //NetClient and -Connection for when joining online matches
        private NetClient joiningClient;
        private NetConnection joiningServerConnection;

        private void Update()
        {
            //J for JOIN
            if (Input.GetKeyDown(KeyCode.J))
            {
                JoinOnlineGame();
            }

            if (joiningClient != null)
            {
                NetIncomingMessage msg;
                while ((msg = joiningClient.ReadMessage()) != null)
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
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                            switch (status)
                            {
                                case NetConnectionStatus.Connected:
                                    Debug.Log("Connected!");
                                    if (msg.SenderConnection.RemoteHailMessage != null)
                                    {
                                        Debug.Log("Server said: " + msg.SenderConnection.RemoteHailMessage.ReadString());
                                        BeginOnlineGame();
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: no hail message");
                                    }
                                    break;

                                default:
                                    string statusMsg = msg.ReadString();
                                    Debug.Log("Status change recieved: " + status + " - Message: " + statusMsg);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public void BeginLocalGame()
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitLocalMatch();
        }

        public void JoinOnlineGame()
        {
            const string IP = "127.0.0.1";
            const int PORT = 25000;

            NetPeerConfiguration conf = new NetPeerConfiguration(APP_ID);
            joiningClient = new NetClient(conf);
            joiningClient.Start();

            //Create approval message
            NetOutgoingMessage approval = joiningClient.CreateMessage();
            approval.Write("Approve me please");

            NetConnection conn = joiningClient.Connect(IP, PORT, approval);
        }

        private void BeginOnlineGame()
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitOnlineMatch(joiningClient, joiningServerConnection);
        }
    }
}