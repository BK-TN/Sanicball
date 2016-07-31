using UnityEngine;

public class Path : MonoBehaviour
{
    public Color pathColor = Color.blue;
    public bool hidePath = false;
    private Transform[] nodes;

    public void SetNodes(Transform[] replacement)
    {
        nodes = replacement;
    }

    public Transform[] GetNodes()
    {
        return nodes;
    }

    // Use this for initialization
    private void Start()
    {
        nodes = new Transform[transform.childCount];
        for (int a = 0; a < transform.childCount; a++)
        {
            nodes[a] = transform.GetChild(a);
        }
    }

    private void OnDrawGizmosSelected()
    {
//        if (nodes != null && !hidePath)
//        {
//            iTween.DrawPath(nodes, pathColor);
//        }
    }
}
