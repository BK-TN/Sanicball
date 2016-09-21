using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    public class AutoStartTimerMessage : MatchMessage
    {
        public bool Enabled { get; private set; }

        public AutoStartTimerMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }
}