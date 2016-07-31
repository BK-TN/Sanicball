using UnityEngine;

namespace Sanicball
{
    public class InternetLink : MonoBehaviour
    {
        public string url;

        public void Visit()
        {
            Application.OpenURL(url);
        }
    }
}
