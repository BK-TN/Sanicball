using UnityEngine;

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

		private int currentColor = 0;
		private Color[] colors = new Color[] { new Color(0, 1, 0, 0.2f), new Color(1, 0, 0, 0.2f), new Color(0, 0, 1, 0.2f), new Color(1, 1, 0, 0.2f) };
		private float colorTimer = 0.25f;

		public void StartTheShit()
		{
			timerOn = true;
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
				GUI.backgroundColor = colors[currentColor];
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
				Rect snoopRect = new Rect(Screen.width - 400, 0, 4, 8);
				GUI.BeginGroup(new Rect(snoopRect.x, snoopRect.y, texture.width * snoopRect.width * ssgui.Size.x, texture.height * snoopRect.height * ssgui.Size.y));
				GUI.DrawTexture(new Rect(-texture.width * snoopRect.width * ssgui.Offset.x, -texture.height * snoopRect.height * ssgui.Offset.y, texture.width * snoopRect.width, texture.height * snoopRect.height), texture);
				GUI.EndGroup();
			}

			if (bass)
			{
				GUIStyle newstyle = new GUIStyle();
				newstyle.alignment = TextAnchor.MiddleCenter;
				newstyle.fontSize = 200;
				newstyle.fontStyle = FontStyle.Bold;
				newstyle.normal.textColor = Color.green;
				GUI.Label(getRekt, "BASS", newstyle);
			}
		}

		private void Update()
		{
			if (Camera.main != null)
			{
				transform.position = Camera.main.transform.position;
				transform.rotation = Camera.main.transform.rotation;
			}
			//transform.FindChild("snoop").Rotate(Vector3.forward*Time.deltaTime*30);

			if (timerOn)
			{
				timer -= Time.deltaTime;

				bass = timer < 0.2f && timer > 0.02f;
				if (timer <= 0)
				{
					timerOn = false;
					screenOverlayEnabled = true;
					//transform.FindChild("snoop").renderer.enabled = true;
				}
			}
			if (screenOverlayEnabled)
			{
				colorTimer -= Time.deltaTime;
				if (colorTimer <= 0)
				{
					currentColor = (currentColor + 1) % colors.Length;
					colorTimer += 0.25f;
				}
			}
		}
	}

}