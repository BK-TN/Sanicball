using System.Collections.Generic;
using UnityEngine;

namespace Sanicball
{
    [ExecuteInEditMode]
    public class Water : MonoBehaviour
    {
        public float noiseScaleX = 1f;
        public float noiseScaleY = 1f;
        public float noiseHeight = 1f;
        public float noiseOffsetX = 0f;
        public float noiseOffsetY = 0f;
        public float triangleSize = 1f;
        public int triangleCount = 64;

        public int noiseIterations = 2;

        public float speed = 1;

        public float[,] points;
        private float t = 0;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        // Use this for initialization
        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            t += Time.deltaTime * 10f;
            t = Time.time * speed;

            points = new float[triangleCount, triangleCount];
            for (var x = 0; x < points.GetLength(0); x++)
            {
                for (var y = 0; y < points.GetLength(1); y++)
                {
                    float nsx = noiseScaleX;
                    float nsy = noiseScaleY;
                    float nh = noiseHeight;
                    for (int i = 0; i < noiseIterations; i++)
                    {
                        points[x, y] += Mathf.PerlinNoise(x * nsx + noiseOffsetX + t, y * nsy + noiseOffsetY) * nh;
                        nsx *= 2;
                        nsy *= 2;
                        nh *= 0.8f;
                    }
                }
            }

            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uv = new List<Vector2>();

            var l = points.GetLength(0);

            for (var x = 0; x < l; x++)
            {
                for (var y = 0; y < l; y++)
                {
                    verts.Add(new Vector3(x * triangleSize - (l / 2) * triangleSize, points[x, y], y * triangleSize - (l / 2) * triangleSize));
                    uv.Add(new Vector2(x / (float)l, y / (float)l));

                    if (x < l - 1 && y < l - 1)
                    {
                        int topLeft = (x % l) + (y * l);
                        int topRight = topLeft + 1;
                        int botLeft = topLeft + l;
                        int botRight = topRight + l;

                        tris.Add(topLeft);
                        tris.Add(topRight);
                        tris.Add(botRight);

                        tris.Add(topLeft);
                        tris.Add(botRight);
                        tris.Add(botLeft);
                    }
                }
            }

            Mesh m = meshFilter.sharedMesh;
            if (m == null)
            {
                m = new Mesh();
            }
            m.name = "waterMesh";
            m.vertices = verts.ToArray();
            m.triangles = tris.ToArray();
            m.uv = uv.ToArray();
            m.RecalculateNormals();
            meshFilter.sharedMesh = m;
        }
    }
}