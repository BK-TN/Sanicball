using Sanicball.Logic;
using UnityEngine;

namespace Sanicball
{
    public class WaitingCamera : MonoBehaviour
    {
        private const float switchTime = 8f;
        private const float moveSpeed = 10f;
        private CameraOrientation[] orientations;

        private int currentOrientation = 0;
        private float timer = switchTime;

        private float vol = 0;

        // Use this for initialization
        private void Start()
        {
            orientations = StageReferences.Active.waitingCameraOrientations;

            AlignWithCurrentOrientation();

            CameraFade.StartAlphaFade(Color.black, true, 4f);
            AudioListener.volume = vol;
        }

        // Update is called once per frame
        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                currentOrientation++;
                if (currentOrientation >= orientations.Length) currentOrientation = 0;

                AlignWithCurrentOrientation();

                timer = switchTime;
            }

            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            if (vol < 1f)
            {
                vol = Mathf.Min(1f, vol + Time.deltaTime / 4);
                AudioListener.volume = Mathf.Lerp(0, Data.ActiveData.GameSettings.soundVolume, vol);
            }
        }

        public void OnDestroy()
        {
            AudioListener.volume = Data.ActiveData.GameSettings.soundVolume;
        }

        private void AlignWithCurrentOrientation()
        {
            transform.position = orientations[currentOrientation].transform.position;
            transform.rotation = orientations[currentOrientation].CameraRotation;
        }
    }
}