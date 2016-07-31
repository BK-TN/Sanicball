using UnityEngine;

[RequireComponent(typeof(PathFollower))]
public class Patroller : MonoBehaviour
{
    public int speed;
    public bool loop;
    public GameObject target;
    private bool isMoving = false;

    private PathFollower pFollower;

    private void Start()
    {
        pFollower = GetComponent<PathFollower>();
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Move();
        }
    }

    private void Move()
    {
        pFollower.Move(speed);
        if (pFollower.position >= 1)
        {
            if (loop) { pFollower.position = 0; } else { speed = 0; }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == target)
        {
            isMoving = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isMoving = false;
    }
}
