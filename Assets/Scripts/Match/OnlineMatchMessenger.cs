using System;
using System.Collections;
using Lidgren.Network;
using Sanicball.Net;
using UnityEngine;

namespace Sanicball.Match
{
    public class OnlineMatchMessenger : MatchMessenger
    {
        public const string APP_ID = "Sanicball";

        private NetClient client;
        private NetConnection serverConnection;

        public OnlineMatchMessenger(NetClient client, NetConnection serverConnection)
        {
            this.client = client;
            this.serverConnection = serverConnection;
        }

        public override void SendMessage<T>(T message)
        {
            NetOutgoingMessage netMessage = client.CreateMessage();

            //TODO: Send match messages over network
        }

        public override void UpdateListeners()
        {
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
                        string statusMsg = msg.ReadString();
                        Debug.Log("Status change recieved: " + status + " - Message: " + statusMsg);
                        break;

                    case NetIncomingMessageType.Data:
                        //TODO: deserialize match messages
                        break;

                    default:
                        Debug.Log("Recieved unhandled message of type " + msg.MessageType);
                        break;
                }
            }
        }
    }
}