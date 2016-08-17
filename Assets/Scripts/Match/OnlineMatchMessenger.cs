using System;
using System.Collections;
using System.Reflection;
using Lidgren.Network;
using UnityEngine;

namespace Sanicball.Match
{
    public class MessageType
    {
        public const byte MatchMessage = 0;
    }

    public class DisconnectArgs : EventArgs
    {
        public string Reason { get; private set; }

        public DisconnectArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class OnlineMatchMessenger : MatchMessenger
    {
        public const string APP_ID = "Sanicball";

        private NetClient client;

        //Settings to use for both serializing and deserializing messages
        private Newtonsoft.Json.JsonSerializerSettings serializerSettings;

        public event EventHandler<DisconnectArgs> Disconnected;

        public OnlineMatchMessenger(NetClient client)
        {
            this.client = client;

            serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
        }

        public override void SendMessage<T>(T message)
        {
            NetOutgoingMessage netMessage = client.CreateMessage();
            netMessage.Write(MessageType.MatchMessage);

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(message, serializerSettings);
            netMessage.Write(data);

            client.SendMessage(netMessage, message.Reliable ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.Unreliable);
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

                        switch (status)
                        {
                            case NetConnectionStatus.Disconnected:
                                if (Disconnected != null)
                                    Disconnected(this, new DisconnectArgs(statusMsg));
                                break;

                            default:
                                Debug.Log("Status change recieved: " + status + " - Message: " + statusMsg);
                                break;
                        }
                        break;

                    case NetIncomingMessageType.Data:

                        switch (msg.ReadByte())
                        {
                            case MessageType.MatchMessage:
                                MatchMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<MatchMessage>(msg.ReadString(), serializerSettings);

                                //Use reflection to call ReceiveMessage with the proper type parameter
                                MethodInfo methodToCall = typeof(OnlineMatchMessenger).GetMethod("RecieveMessage", BindingFlags.NonPublic | BindingFlags.Instance);
                                MethodInfo genericVersion = methodToCall.MakeGenericMethod(message.GetType());
                                genericVersion.Invoke(this, new[] { message });

                                break;
                        }
                        break;

                    default:
                        Debug.Log("Recieved unhandled message of type " + msg.MessageType);
                        break;
                }
            }
        }

        public override void Close()
        {
            client.Disconnect("Client closed the game.");
        }

        private void RecieveMessage<T>(T message) where T : MatchMessage
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                MatchMessageListener listener = listeners[i];
                if (listener.MessageType == message.GetType())
                {
                    ((MatchMessageHandler<T>)listener.Handler).Invoke(message);
                }
            }
        }
    }
}