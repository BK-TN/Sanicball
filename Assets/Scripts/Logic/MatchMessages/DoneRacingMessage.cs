using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    public class DoneRacingMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }

        public DoneRacingMessage(System.Guid clientGuid, ControlType ctrlType)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
        }
    }
}