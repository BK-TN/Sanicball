using System.Collections;
using UnityEngine;

public interface IBallCamera
{
    Rigidbody Target { get; set; }
    Camera Cam { get; }

    void SetDirection(Quaternion dir);
}