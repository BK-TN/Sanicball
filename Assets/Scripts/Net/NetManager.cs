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

        public void Connect(string ip, int port)
        {
            NetPeerConfiguration conf = new NetPeerConfiguration(APP_ID);
            client = new NetClient(conf);
            client.Start();
            NetOutgoingMessage approval = client.CreateMessage();
            approval.Write("Approve me please");
            conn = client.Connect(ip, port, approval);
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
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                Debug.Log("Connected!");
                                if (msg.SenderConnection.RemoteHailMessage != null)
                                {
                                    Debug.Log("Server said: " + msg.SenderConnection.RemoteHailMessage.ReadString());
                                    matchManager = Instantiate(matchManagerPrefab);
                                    matchManager.InitOnlineMatch();
                                    matchManager.SettingsChangeRequested += MatchManager_SettingsChangeRequested;
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

                    case NetIncomingMessageType.Data:
                        byte type = msg.ReadByte();
                        switch (type)
                        {
                            case MessageType.MatchSettingsChanged:
                                Debug.Log("New match settings recieved from server");
                                string data = msg.ReadString();
                                Data.MatchSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.MatchSettings>(data);
                                matchManager.ChangeSettings(settings);
                                break;
                        }
                        break;

                    default:
                        Debug.Log("Recieved unhandled message of type " + msg.MessageType);
                        break;
                }
            }
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