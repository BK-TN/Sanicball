using System;
using System.Collections;
using Lidgren.Network;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Logic
{
    public class PlayerMovement

    {
        public Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }
        public Vector3 DirectionVector { get; private set; }

        public PlayerMovement(Guid clientGuid, ControlType ctrlType, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 directionVector)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            DirectionVector = directionVector;
        }

        public static PlayerMovement CreateFromPlayer(MatchPlayer player)
        {
            Rigidbody rigidbody = player.BallObject.GetComponent<Rigidbody>();
            return new PlayerMovement(
                player.ClientGuid,
                player.CtrlType,
                player.BallObject.transform.position,
                player.BallObject.transform.rotation,
                rigidbody.velocity,
                rigidbody.angularVelocity,
                player.BallObject.DirectionVector
                );
        }

        public void WriteToMessage(NetBuffer msg)
        {
            msg.Write(ClientGuid);
            msg.Write((byte)CtrlType);
            msg.Write(Position);
            msg.Write(Rotation);
            msg.Write(Velocity);
            msg.Write(AngularVelocity);
            msg.Write(DirectionVector);
        }

        public static PlayerMovement ReadFromMessage(NetBuffer msg)
        {
            return new PlayerMovement(
                msg.ReadGuid(),
                (ControlType)msg.ReadByte(),
                msg.ReadVector3(),
                msg.ReadQuaternion(),
                msg.ReadVector3(),
                msg.ReadVector3(),
                msg.ReadVector3()
                );
        }
    }
}