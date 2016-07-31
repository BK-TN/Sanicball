using UnityEngine;

public class DestroyOnWeb : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsWebPlayer ||
            Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            Destroy(this.gameObject);
        }
    }
}
