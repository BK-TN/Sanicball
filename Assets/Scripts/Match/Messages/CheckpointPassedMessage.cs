using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public class CheckpointPassedMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public float LapTime { get; private set; }

        public CheckpointPassedMessage(System.Guid clientGuid, ControlType ctrlType, float lapTime)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            LapTime = lapTime;
        }
    }
}