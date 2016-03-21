using System.Collections;
using UnityEngine;

namespace Sanicball
{
    public static class UISound
    {
        private static AudioSource instance;

        public static void Play(AudioClip clip)
        {
            if (!instance)
            {
                instance = new GameObject("UI Sound").AddComponent<AudioSource>();
            }
            instance.clip = clip;
            instance.Play();
        }
    }
}
