using UnityEngine;

namespace Sanicball
{
    public class CycleMaterial : MonoBehaviour
    {
        public Material[] materials;
        private int current;

        public void Switch()
        {
            current++;
            if (current >= materials.Length)
                current = 0;
            GetComponent<Renderer>().material = materials[current];
        }
    }
}