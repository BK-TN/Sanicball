using Lidgren.Network;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Logic
{
    public class MatchStarter : MonoBehaviour
    {
        public const string APP_ID = "Sanicball";

        [SerializeField]
        private MatchManager matchManagerPrefab = null;
        [SerializeField]
        private UI.Popup connectingPopupPrefab = null;
        [SerializeField]
        private UI.PopupHandler popupHandler = null;

        private UI.PopupConnecting activeConnectingPopup;

        //NetClient for when joining online matches
        private NetClient joiningClient;

        private void Update()
        {
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
                                    Debug.Log("Connected! Now waiting for match state");
                                    activeConnectingPopup.ShowMessage("Receiving match state...");
                                    break;

                                case NetConnectionStatus.Disconnected:
                                    activeConnectingPopup.ShowMessage(msg.ReadString());
                                    break;

                                default:
                                    string statusMsg = msg.ReadString();
                                    Debug.Log("Status change received: " + status + " - Message: " + statusMsg);
                                    break;
                            }
                            break;

                        case NetIncomingMessageType.Data:
                            byte type = msg.ReadByte();
                            if (type == MessageType.InitMessage)
                            {
                                try
                                {
                                    MatchState matchInfo = MatchState.ReadFromMessage(msg);
                                    BeginOnlineGame(matchInfo);
                                }
                                catch (System.Exception ex)
                                {
                                    activeConnectingPopup.ShowMessage("Failed to read match message - cannot join server!");
                                    Debug.LogError("Could not read match state, error: " + ex.Message);
                                }

                                /*string matchStateStr = "";
                                try
                                {
                                    matchStateStr = msg.ReadString();
                                    MatchState matchInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MatchState>(matchStateStr);
                                    BeginOnlineGame(matchInfo);
                                }
                                catch (Newtonsoft.Json.JsonException ex)
                                {
                                    activeConnectingPopup.ShowMessage("Failed to read match state - cannot join server!");
                                    joiningClient.Disconnect("Failed to read match state");
                                    Debug.LogError("Could not read match state, error: " + ex.Message);
                                    Debug.LogError("Full message: " + matchStateStr);
                                }*/
                            }
                            break;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    popupHandler.CloseActivePopup();
                    joiningClient.Disconnect("Cancelled");
                    joiningClient = null;
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
            JoinOnlineGame(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
        }

        public void JoinOnlineGame(System.Net.IPEndPoint endpoint)
        {
            NetPeerConfiguration conf = new NetPeerConfiguration(APP_ID);
            joiningClient = new NetClient(conf);
            joiningClient.Start();

            //Create approval message
            NetOutgoingMessage approval = joiningClient.CreateMessage();

            ClientInfo info = new ClientInfo(GameVersion.AS_FLOAT, GameVersion.IS_TESTING);
            approval.Write(Newtonsoft.Json.JsonConvert.SerializeObject(info));

            joiningClient.Connect(endpoint, approval);

            popupHandler.OpenPopup(connectingPopupPrefab);

            activeConnectingPopup = FindObjectOfType<UI.PopupConnecting>();
        }

        //Called when succesfully connected to a server
        private void BeginOnlineGame(MatchState matchState)
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitOnlineMatch(joiningClient, matchState);
        }
    }
}