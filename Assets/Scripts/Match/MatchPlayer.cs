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
        private DateTime latestMovementMessageTime = DateTime.Now;

        public MatchPlayer(Guid clientGuid, ControlType ctrlType, int initialCharacterId)
        {
            this.clientGuid = clientGuid;
            this.ctrlType = ctrlType;
            CharacterId = initialCharacterId;
        }

        public Guid ClientGuid { get { return clientGuid; } }
        public ControlType CtrlType { get { return ctrlType; } }
        public int CharacterId { get; set; }
        public Ball BallObject { get; set; }
        public bool ReadyToRace { get; set; }

        public void ProcessMovementMessage(PlayerMovementMessage msg)
        {
            if (msg.Timestamp > latestMovementMessageTime)
            {
                Rigidbody ballRb = BallObject.GetComponent<Rigidbody>();

                BallObject.transform.position = msg.Position.ToVector3();
                BallObject.transform.rotation = Quaternion.Euler(msg.Rotation.ToVector3());
                ballRb.velocity = msg.Velocity.ToVector3();
                ballRb.angularVelocity = msg.AngularVelocity.ToVector3();
                BallObject.DirectionVector = msg.DirectionVector.ToVector3();

                latestMovementMessageTime = msg.Timestamp;
            }
        }
    }
}