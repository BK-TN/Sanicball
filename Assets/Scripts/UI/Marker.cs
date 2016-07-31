using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Marker : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Text textField;

    public Transform Target { get { return target; } set { target = value; } }
    public string Text { get { return textField.text; } set { textField.text = value; } }
    public Camera CameraToUse { get; set; }

    private RectTransform rectTransform;

    private const float clampMin = 0.1f;
    private const float clampMax = 1 - clampMin;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()	
    {
        if (!target) return;

        var cam = Camera.main;
        if (CameraToUse) cam = CameraToUse;

        var relativePosition = cam.transform.InverseTransformPoint(target.position);
        relativePosition.z = Mathf.Max(relativePosition.z, 1);

        Vector3 p = cam.WorldToViewportPoint(cam.transform.TransformPoint(relativePosition));

        p = new Vector3(Mathf.Clamp(p.x, clampMin, clampMax), Mathf.Clamp(p.y, clampMin, clampMax), p.z);

        rectTransform.anchorMin = p;
        rectTransform.anchorMax = p;
    }
}