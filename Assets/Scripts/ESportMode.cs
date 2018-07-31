using UnityEngine;
using System.Collections.Generic;

namespace Sanicball {
	public class ESportMode : MonoBehaviour
	{
		public Texture2D screenOverlay;
		public Texture2D solidWhite;
		public Texture2D snoop;

		private bool screenOverlayEnabled = false;
		private bool timerOn = false;
		private bool bass = false;
		private float timer = 1f;
		bool started = false;

		private Color currentColor = new Color(1,0,0,0.2f);
		const float COLOR_TIME = 60.0f / 110.0f;
		private float colorTimer = COLOR_TIME;

		Vector2 snoopPos = new Vector2(0,0);
		Vector2 snoopTarget = new Vector2(0,0);

		//Groove
		private int qSamples = 1024; // array size
		private float refValue = 0.1f; // RMS value for 0 dB
		private float rmsValue; // sound level - RMS
		private float dbValue; // sound level - dB
		private float[] samples; // audio samples
		float RMSmin = 0f;
		float RMSmax = 0f;

		void Start()
		{
			samples = new float[qSamples];
		}

		public void StartTheShit()
		{
			timerOn = true;
		}

		List<Camera> cameras = new List<Camera>();
		AudioSource music;

		private void Start4Real()
		{
			started = true;
			screenOverlayEnabled = true;
			foreach (CameraEffects e in FindObjectsOfType<CameraEffects>())
			{
				cameras.Add(e.GetComponent<Camera>());
				e.bloom.bloomIntensity = 2f;
				e.bloom.bloomThreshold = 0.6f;
				e.blur.velocityScale = 4;
			}
			music = FindObjectOfType<MusicPlayer>().GetComponent<AudioSource>();
		}

		//Groove
		void GetVolume(){
			music.GetOutputData(samples, 0); // fill array with samples
			float sum = 0f;
			for (var i=0; i < qSamples; i++){
				sum += samples[i]*samples[i]; // sum squared samples
			}
			rmsValue = Mathf.Sqrt(sum/qSamples); // rms = square root of average
			dbValue = 20*Mathf.Log10(rmsValue/refValue); // calculate dB
			if (dbValue < -160) dbValue = -160; // clamp it to -160dB min
		}

		private void Update()
		{
			if (Camera.main != null)
			{
				transform.position = Camera.main.transform.position;
				transform.rotation = Camera.main.transform.rotation;
			}

			if (timerOn)
			{
				timer -= Time.deltaTime;

				bass = timer < 0.2f && timer > 0.02f;
				if (timer <= 0)
				{
					timerOn = false;
					Start4Real();
				}
			}
			if (screenOverlayEnabled)
			{
				snoopPos = Vector2.MoveTowards(snoopPos, snoopTarget, Time.deltaTime * 32);
				if (snoopPos == snoopTarget)
				{
					snoopTarget = new Vector2(Random.Range(0,Screen.width), Random.Range(0,Screen.height));
				}

				colorTimer -= Time.deltaTime;
				if (colorTimer <= 0)
				{
					currentColor = new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f), 0.2f);
					colorTimer += COLOR_TIME;
				}
			}

			if (started)
			{
				//Groove
				GetVolume();
				RMSmin = Mathf.Min(RMSmin,rmsValue);
				RMSmax = Mathf.Max(RMSmax,rmsValue);
				Camera.main.backgroundColor = Color.Lerp(Color.magenta,Color.blue,rmsValue);

				var fov = 20 - rmsValue * 80;
				foreach(Camera c in cameras)
				{
					var omni = c.GetComponent<Sanicball.Gameplay.OmniCamera>();
					if (omni)
					{
						omni.fovOffset = fov;
					}
					else
					{
						c.fieldOfView = Mathf.Lerp(c.fieldOfView, 72 + fov, Time.deltaTime * 20);
					}
				}
			}
		}

		private void OnGUI()
		{
			Rect getRekt = new Rect(0, 0, Screen.width, Screen.height);

			if (screenOverlayEnabled)
			{
				//Background
				GUIStyle colorStyle = new GUIStyle();
				colorStyle.normal.background = solidWhite;
				colorStyle.stretchWidth = true;
				colorStyle.stretchHeight = true;
				GUI.backgroundColor = currentColor;
				GUI.Box(getRekt, "", colorStyle);
				GUI.backgroundColor = Color.white;

				//Overlay
				GUIStyle mlgStyle = new GUIStyle();
				mlgStyle.normal.background = screenOverlay;
				mlgStyle.stretchWidth = true;
				mlgStyle.stretchHeight = true;
				GUI.Box(getRekt, "", mlgStyle);

				//Snoop
				SpriteSheetGUI ssgui = GetComponent<SpriteSheetGUI>();
				Texture2D texture = snoop;
				Rect snoopRect = new Rect(snoopPos.x, snoopPos.y, 8, 16);
				GUI.BeginGroup(new Rect(snoopRect.x, snoopRect.y, texture.width * snoopRect.width * ssgui.Size.x, texture.height * snoopRect.height * ssgui.Size.y));
				GUI.color = new Color(1,1,1,0.4f);
				GUI.DrawTexture(new Rect(-texture.width * snoopRect.width * ssgui.Offset.x, -texture.height * snoopRect.height * ssgui.Offset.y, texture.width * snoopRect.width, texture.height * snoopRect.height), texture);
				GUI.EndGroup();
			}

			if (bass)
			{
				GUIStyle newstyle = new GUIStyle();
				newstyle.alignment = TextAnchor.MiddleCenter;
				newstyle.fontSize = 600;
				newstyle.fontStyle = FontStyle.Bold;
				newstyle.normal.textColor = new Color(0,1,0,0.5f);
				GUI.Label(getRekt, "BASS", newstyle);
			}
		}

	}

}