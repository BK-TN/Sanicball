using UnityEngine;

[RequireComponent(typeof(PathFollower))]

public class PathPatroller : MonoBehaviour
{
    public float speed;
    public float accel;
    public float maxSpeed;
    public bool loop;

    // Use this for initialization
    private void FixedUpdate()
    {
        Move();
        if (speed < maxSpeed)
        {
            speed = Mathf.Clamp(speed + accel, 0, maxSpeed);
        }
    }

    // Update is called once per frame
    private void Move()
    {
        PathFollower pFollower = GetComponent<PathFollower>();
        pFollower.Move(speed);
        if (pFollower.position >= 1)
        {
            if (loop) { pFollower.position = 0; } else { speed = 0; }
        }
    }
}
