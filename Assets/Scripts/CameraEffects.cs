using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using Sanicball.Data;

namespace Sanicball {
	public class CameraEffects : MonoBehaviour {


		public bool isOmniCam = false;

		public Texture2D bloomLensFlareVignetteMask;
		public Shader bloomLensFlareShader;
		public Shader bloomScreenBlendShader;
		public Shader bloomBlurAndFlaresShader;
		public Shader bloomBrightPassFilter;

		public Shader motionBlurShader;
		public Shader motionBlurDX11Shader;
		public Shader motionBlurReplacementClearShader;
		public Texture2D motionBlurNoiseTexture;

		[System.NonSerialized]
		public Bloom bloom;
		[System.NonSerialized]
		public CameraMotionBlur blur;

		// Use this for initialization
		void Start () {
			bloom = gameObject.AddComponent<Bloom>();
			bloom.bloomIntensity = 0.8f;
			bloom.bloomThreshold = 0.8f;

			bloom.lensFlareVignetteMask = bloomLensFlareVignetteMask;
			bloom.lensFlareShader = bloomLensFlareShader;
			bloom.screenBlendShader = bloomScreenBlendShader;
			bloom.blurAndFlaresShader = bloomBlurAndFlaresShader;
			bloom.brightPassFilterShader = bloomBrightPassFilter;

			blur = gameObject.AddComponent<CameraMotionBlur>();
			blur.filterType = CameraMotionBlur.MotionBlurFilter.LocalBlur;
			blur.velocityScale = 1;
			blur.maxVelocity = 1000;
			blur.minVelocity = 0.1f;

			blur.shader = motionBlurShader;
			blur.dx11MotionBlurShader = motionBlurDX11Shader;
			blur.replacementClear = motionBlurReplacementClearShader;
			blur.noiseTexture = motionBlurNoiseTexture;

			if (ActiveData.ESportsFullyReady)
			{
			}

			EnableEffects();
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void EnableEffects()
		{
			bloom.enabled = ActiveData.GameSettings.bloom;
			blur.enabled = ActiveData.GameSettings.motionBlur;
		}
	}
}