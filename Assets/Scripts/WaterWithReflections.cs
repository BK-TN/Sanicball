using System.Collections;
using Sanicball.Data;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(MeshRenderer))]
    public class WaterWithReflections : MonoBehaviour
    {
        [SerializeField]
        private MirrorReflection reflection;

        [SerializeField]
        private float materialAlphaWithReflections = 0.8f;

        private void Start()
        {
            ReflectionQuality q = ActiveData.GameSettings.reflectionQuality;

            if (q == ReflectionQuality.Off)
            {
                reflection.enabled = false;
            }
            else
            {
                reflection.enabled = true;
                Color c = GetComponent<MeshRenderer>().material.color;
                c.a = materialAlphaWithReflections;
                GetComponent<MeshRenderer>().material.color = c;
                switch (q)
                {
                    case ReflectionQuality.Low:
                        reflection.m_TextureSize = 256;
                        break;

                    case ReflectionQuality.Medium:
                        reflection.m_TextureSize = 512;
                        break;

                    case ReflectionQuality.High:
                        reflection.m_TextureSize = 1024;
                        break;
                }
            }
        }

        private void Update()
        {
        }
    }
}