using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public struct SimpleVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SimpleVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    public static class Vector3Extensions
    {
        public static SimpleVector3 ToSimpleVector3(this Vector3 vector)
        {
            return new SimpleVector3(vector.x, vector.y, vector.z);
        }
    }

    public class PlayerMovementMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public SimpleVector3 Position { get; private set; }
        public SimpleVector3 Rotation { get; private set; }
        public SimpleVector3 Velocity { get; private set; }
        public SimpleVector3 AngularVelocity { get; private set; }
        public SimpleVector3 DirectionVector { get; private set; }

        public PlayerMovementMessage(System.Guid clientGuid, ControlType ctrlType, SimpleVector3 position, SimpleVector3 rotation, SimpleVector3 velocity, SimpleVector3 angularVelocity, SimpleVector3 directionVector)
        {
            reliable = false;

            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            DirectionVector = directionVector;
        }
    }
}