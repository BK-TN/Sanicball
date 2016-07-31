using UnityEngine;

[RequireComponent(typeof(Path))]

public class PathRecorder : MonoBehaviour
{
    public Transform objectToRecord;
    public Vector3 lastPoint;
    public Object nodeObject;
    //int timer=25;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //timer--;
        if (Vector3.Distance(lastPoint, objectToRecord.position) > 50)
        {
            GameObject g = (GameObject)Instantiate(nodeObject, objectToRecord.position, Quaternion.Euler(Vector3.zero));
            g.transform.parent = transform;
            lastPoint = objectToRecord.position;
            //timer = 25;
        }
    }
}
