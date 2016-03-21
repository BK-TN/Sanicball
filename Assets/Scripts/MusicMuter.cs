using System.Collections;
using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicMuter : MonoBehaviour
    {
        private AudioSource aSource;

        private void Start()
        {
            aSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            aSource.mute = !Data.ActiveData.GameSettings.music;
        }
    }
}