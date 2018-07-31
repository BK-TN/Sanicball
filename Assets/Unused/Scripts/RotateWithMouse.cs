using UnityEngine;

//Was supposed to be used with race overviews
public class RotateWithMouse : MonoBehaviour
{
    public Vector3 baseRotation;
    public float xIntensity = 1f;
    public float yIntensity = 1f;
    public float speed = 1f;

    private Quaternion targetRotation = Quaternion.identity;

    // Use this for initialization
    private void Start()
    {
        targetRotation = Quaternion.Euler(baseRotation);
    }

    // Update is called once per frame
    private void Update()
    {
        float x = Mathf.Lerp(-1f, 1f, Input.mousePosition.x / Screen.width);
        float y = Mathf.Lerp(-1f, 1f, Input.mousePosition.y / Screen.height);

        targetRotation = Quaternion.Euler(baseRotation) * Quaternion.Euler(-y * yIntensity, 0, x * xIntensity);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }
}