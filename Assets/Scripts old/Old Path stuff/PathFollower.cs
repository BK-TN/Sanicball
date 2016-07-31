using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public bool followX;
    public bool followY;
    public bool followZ;

    public Path path;

    public float position;

    public void Move(float speed)
    {
        float a = speed * CalcPathDistortionAtPoint(position);
        float b = a / iTween.PathLength(path.GetNodes());
        position += b * Time.deltaTime;
        if (position > 1) { position = 1; }
        if (position < 0) { position = 0; }
    }

    public float GetPositionPercentile()
    {
        return position;
    }

    public Vector3 GetPosition()
    {
        return GetPathPoint(position);
    }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().MovePosition(GetPathPoint(position));
        }
        else {
            transform.position = GetPathPoint(position);
        }
        iTween.LookUpdate(this.gameObject, GetPathPoint(position + 0.01f), 0.2f);
    }

    private float CalcPathDistortionAtPoint(float pos)
    {
        Vector3 point = GetPathPoint(pos);
        Vector3 point2 = GetPathPoint(pos + 0.01f);
        float dist = Vector3.Distance(point, point2);
        return (iTween.PathLength(path.GetNodes()) / 100) / dist;
    }

    private Vector3 GetPathPoint(float pos)
    {
        Vector3 a = iTween.PointOnPath(path.GetNodes(), pos);
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        if (followX) { x = a.x; }
        if (followY) { y = a.y; }
        if (followZ) { z = a.z; }
        return new Vector3(x, y, z);
    }
}
