using System;
using System.Collections;
using System.Reflection;
using Lidgren.Network;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Logic
{
    public class DisconnectArgs : EventArgs
    {
        public string Reason { get; private set; }

        public DisconnectArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class PlayerMovementArgs : EventArgs
    {
        public double Timestamp { get; private set; }
        public PlayerMovement Movement { get; private set; }

        public PlayerMovementArgs(double timestamp, PlayerMovement movement)
        {
            Timestamp = timestamp;
            Movement = movement;
        }
    }

    public class OnlineMatchMessenger : MatchMessenger
    {
        public const string APP_ID = "Sanicball";

        private NetClient client;

        //Settings to use for both serializing and deserializing messages
        private Newtonsoft.Json.JsonSerializerSettings serializerSettings;

        public event EventHandler<PlayerMovementArgs> OnPlayerMovement;
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
            netMessage.WriteTime(false);

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(message, serializerSettings);
            netMessage.Write(data);

            client.SendMessage(netMessage, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendPlayerMovement(MatchPlayer player)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write(MessageType.PlayerMovementMessage);
            msg.WriteTime(false);
            PlayerMovement movement = Logic.PlayerMovement.CreateFromPlayer(player);
            movement.WriteToMessage(msg);
            client.SendMessage(msg, NetDeliveryMethod.Unreliable);
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
                                Debug.Log("Status change received: " + status + " - Message: " + statusMsg);
                                break;
                        }
                        break;

                    case NetIncomingMessageType.Data:

                        switch (msg.ReadByte())
                        {
                            case MessageType.MatchMessage:
                                double timestamp = msg.ReadTime(false);
                                MatchMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<MatchMessage>(msg.ReadString(), serializerSettings);

                                //Use reflection to call ReceiveMessage with the proper type parameter
                                MethodInfo methodToCall = typeof(OnlineMatchMessenger).GetMethod("ReceiveMessage", BindingFlags.NonPublic | BindingFlags.Instance);
                                MethodInfo genericVersion = methodToCall.MakeGenericMethod(message.GetType());
                                genericVersion.Invoke(this, new object[] { message, timestamp });

                                break;

                            case MessageType.PlayerMovementMessage:
                                double time = msg.ReadTime(false);
                                PlayerMovement movement = PlayerMovement.ReadFromMessage(msg);
                                if (OnPlayerMovement != null)
                                {
                                    OnPlayerMovement(this, new PlayerMovementArgs(time, movement));
                                }
                                break;
                        }
                        break;

                    default:
                        Debug.Log("Received unhandled message of type " + msg.MessageType);
                        break;
                }
            }
        }

        public override void Close()
        {
            client.Disconnect("Client left the match");
        }

        private void ReceiveMessage<T>(T message, double timestamp) where T : MatchMessage
        {
            float travelTime = (float)(NetTime.Now - timestamp);

            for (int i = 0; i < listeners.Count; i++)
            {
                MatchMessageListener listener = listeners[i];
                if (listener.MessageType == message.GetType())
                {
                    ((MatchMessageHandler<T>)listener.Handler).Invoke(message, travelTime);
                }
            }
        }
    }
}