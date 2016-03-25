using System.Collections;
using UnityEngine;

public interface IBallCamera
{
    Rigidbody Target { get; set; }
    Camera AttachedCamera { get; }

    void SetDirection(Quaternion dir);
}