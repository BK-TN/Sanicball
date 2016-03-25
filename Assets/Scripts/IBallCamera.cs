using System.Collections;
using UnityEngine;

namespace Sanicball
{
    public interface IBallCamera
    {
        Rigidbody Target { get; set; }
        Camera AttachedCamera { get; }
        ControlType CtrlType { get; set; }

        void SetDirection(Quaternion dir);
    }
}