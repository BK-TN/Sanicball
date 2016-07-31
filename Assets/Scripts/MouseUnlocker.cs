using UnityEngine;

namespace Sanicball
{
    public class MouseUnlocker : MonoBehaviour
    {
        private void Start()
        {
            if (FindObjectsOfType<MouseUnlocker>().Length > 1)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
