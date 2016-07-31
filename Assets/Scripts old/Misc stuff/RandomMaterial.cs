using UnityEngine;

public class RandomMaterial : MonoBehaviour
{
    public Material[] materials;
    private int current;

    // Update is called once per frame
    public void Switch()
    {
        current++;
        if (current >= materials.Length)
            current = 0;
        GetComponent<Renderer>().material = materials[current];
    }

    // Use this for initialization
    private void Start()
    {
    }
}
