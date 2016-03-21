using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Screenshot captured.");
            Application.CaptureScreenshot("screenshot.png", 2);
        }
    }
}
