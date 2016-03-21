using UnityEngine;

[RequireComponent(typeof(Path))]
[ExecuteInEditMode()]

public class PathUpdater : MonoBehaviour
{
    public bool loop;
    public bool disable = false;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (disable) return;
        if (transform.childCount > 0)
        {
            int b = transform.childCount;
            if (loop)
                b += 1;
            Transform[] newnodes = new Transform[b];
            for (int a = 0; a < transform.childCount; a++)
                newnodes[a] = transform.GetChild(a);
            if (loop)
                newnodes[transform.childCount] = transform.GetChild(0);

            Path p = GetComponent<Path>();
            p.SetNodes(newnodes);
        }
        else {
            Path p = GetComponent<Path>();
            p.SetNodes(new Transform[] { transform, transform });
        }
    }
}
