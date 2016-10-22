using System.Collections;
using System.Linq;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(Camera))]
    public class CameraSplitter : MonoBehaviour
    {
        private Camera cam;
        private AudioListener listener;

        public int SplitscreenIndex { get; set; }

        private void Start()
        {
            cam = GetComponent<Camera>();
            listener = GetComponent<AudioListener>();
            var splitters = FindObjectsOfType<CameraSplitter>().ToList();

            int count = splitters.Count;
            int index = SplitscreenIndex;

            switch (count)
            {
                case 2:
                    cam.rect = new Rect(0, index == 0 ? 0.5f : 0f, 1, 0.5f);
                    break;

                case 3:
                    switch (index)
                    {
                        case 0:
                            cam.rect = new Rect(0, 0.5f, 1, 0.5f);
                            break;

                        case 1:
                            cam.rect = new Rect(0, 0, 0.5f, 0.5f);
                            break;

                        case 2:
                            cam.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                            break;
                    }
                    break;

                case 4:
                    switch (index)
                    {
                        case 0:
                            cam.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                            break;

                        case 1:
                            cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                            break;

                        case 2:
                            cam.rect = new Rect(0, 0, 0.5f, 0.5f);
                            break;

                        case 3:
                            cam.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                            break;
                    }
                    break;

                default:
                    cam.rect = new Rect(0, 0, 1, 1);
                    break;
            }

            if (listener)
            {
                listener.enabled = index == 0;
            }
        }
    }
}