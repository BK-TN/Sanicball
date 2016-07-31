using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Sanicball
{
    [RequireComponent(typeof(Camera))]
    public class BlurCameraOnPause : MonoBehaviour
    {
        private const string pauseTag = "Pause";

        [SerializeField]
        private Shader shader;

        private bool paused = false;
        private BlurOptimized blur;

        private const float targetBlurSize = 5f;

        private void Start()
        {
        }

        private void Update()
        {
            if (!paused)
            {
                if (UI.PauseMenu.GamePaused)
                {
                    paused = true;
                    blur = gameObject.AddComponent<BlurOptimized>();
                    blur.downsample = 1;
                    blur.blurSize = 0;
                    blur.blurIterations = 3;
                    blur.blurType = BlurOptimized.BlurType.StandardGauss;
                    blur.blurShader = shader;
                }
            }
            else
            {
                if (blur.blurSize < targetBlurSize)
                {
                    blur.blurSize = Mathf.Min(blur.blurSize + Time.unscaledDeltaTime * 40, targetBlurSize);
                }

                if (!UI.PauseMenu.GamePaused)
                {
                    paused = false;
                    Destroy(blur);
                }
            }
        }
    }
}