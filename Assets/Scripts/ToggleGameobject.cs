using UnityEngine;

namespace Sanicball
{
    //This script is used for toggling menu windows using Unity's built-in event system.
    public class ToggleGameobject : MonoBehaviour
    {
        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}