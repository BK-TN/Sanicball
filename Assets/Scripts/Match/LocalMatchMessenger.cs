using System;
using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public class LocalMatchMessenger : MatchMessenger
    {
        public override void SendMessage<T>(T message)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                MatchMessageListener listener = listeners[i];
                if (listener.Type == message.GetType())
                {
                    ((MatchMessageHandler<T>)listener.Handler).Invoke(message);
                }
            }
        }

        public override void UpdateListeners()
        {
        }
    }
}