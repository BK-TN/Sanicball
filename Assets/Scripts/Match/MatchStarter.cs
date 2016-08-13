using Lidgren.Network;
using UnityEngine;

namespace Sanicball.Match
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
                                        string hailMsg = msg.SenderConnection.RemoteHailMessage.ReadString();
                                        Debug.Log("Server said: " + hailMsg);
                                        MatchState matchInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MatchState>(hailMsg);
                                        BeginOnlineGame(matchInfo);
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: no hail message");
                                    }
                                    break;

                                case NetConnectionStatus.Disconnected:
                                    Debug.Log("Disconnected, shit");
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

        public void JoinOnlineGame(string ip = "127.0.0.1", int port = 25000)
        {
            NetPeerConfiguration conf = new NetPeerConfiguration(APP_ID);
            joiningClient = new NetClient(conf);
            joiningClient.Start();

            //Create approval message
            NetOutgoingMessage approval = joiningClient.CreateMessage();
            approval.Write("Approve me please");

            joiningClient.Connect(ip, port, approval);
        }

        //Called when succesfully connected to a server
        private void BeginOnlineGame(MatchState matchState)
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitOnlineMatch(joiningClient, joiningServerConnection, matchState);
        }
    }
}