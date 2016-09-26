using System.Collections;
using SanicballCore;
using UnityEngine;

namespace Sanicball.Gameplay
{
    public interface IBallCamera
    {
        Rigidbody Target { get; set; }
        Camera AttachedCamera { get; }
        ControlType CtrlType { get; set; }

        void SetDirection(Quaternion dir);

        void Remove();
    }
}