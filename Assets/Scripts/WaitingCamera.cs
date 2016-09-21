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

        // Use this for initialization
        private void Start()
        {
            orientations = StageReferences.Active.waitingCameraOrientations;

            AlignWithCurrentOrientation();

            CameraFade.StartAlphaFade(Color.black, true, 4f);
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
        }

        private void AlignWithCurrentOrientation()
        {
            transform.position = orientations[currentOrientation].transform.position;
            transform.rotation = orientations[currentOrientation].CameraRotation;
        }
    }
}
