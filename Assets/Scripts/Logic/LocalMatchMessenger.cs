using System;
using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    public class LocalMatchMessenger : MatchMessenger
    {
        public override void SendMessage<T>(T message)
        {
            Debug.Log("Recieved message of type " + typeof(T));
            for (int i = 0; i < listeners.Count; i++)
            {
                MatchMessageListener listener = listeners[i];
                if (listener.MessageType == message.GetType())
                {
                    ((MatchMessageHandler<T>)listener.Handler).Invoke(message);
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