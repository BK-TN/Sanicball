using System;
using System.Collections;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Logic
{
    public class LocalMatchMessenger : MatchMessenger
    {
        public override void SendMessage<T>(T message)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                MatchMessageListener listener = listeners[i];
                if (listener.MessageType == message.GetType())
                {
                    ((MatchMessageHandler<T>)listener.Handler).Invoke(message, 0);
                }
            }
        }

        public override void UpdateListeners()
        {
        }

        public override void Close()
        {
        }
    }
}