using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Sanicball.Match
{
    public class MatchPlayerEventArgs : EventArgs
    {
        public MatchPlayer Player { get; private set; }
        public bool IsLocal { get; private set; }

        public MatchPlayerEventArgs(MatchPlayer player, bool isLocal)
        {
            Player = player;
            IsLocal = isLocal;
        }
    }

    [Serializable]
    public class MatchPlayer
    {
        private Guid clientGuid;
        private ControlType ctrlType;
        private bool readyToRace;

        public MatchPlayer(Guid clientGuid, ControlType ctrlType, int initialCharacterId)
        {
            this.clientGuid = clientGuid;
            this.ctrlType = ctrlType;
            CharacterId = initialCharacterId;
        }

        public event EventHandler ChangedReady;

        public Guid ClientGuid { get { return clientGuid; } }
        public ControlType CtrlType { get { return ctrlType; } }
        public int CharacterId { get; set; }
        public Ball BallObject { get; set; }
        public bool ReadyToRace
        {
            get { return readyToRace; }
            set
            {
                readyToRace = value;
                if (ChangedReady != null)
                    ChangedReady(this, EventArgs.Empty);
            }
        }
    }
}