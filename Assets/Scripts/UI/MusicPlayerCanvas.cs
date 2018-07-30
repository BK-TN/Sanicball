using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Sanicball.UI 
{
	public class MusicPlayerCanvas : MonoBehaviour {

		public CanvasGroup panel;
		public Text label;
		public bool lobbyOffset = false;

		float showTimer = 0;

		void Start () {
			panel.alpha = 0;
			if (lobbyOffset)
			{
				var rt = panel.GetComponent<RectTransform>();
				var p = rt.anchoredPosition;
				p.y += 48;
				rt.anchoredPosition = p;
			}
		}
		
		void Update () {
			if (showTimer > 0)
			{
				panel.alpha = Mathf.Lerp(panel.alpha, 1, Time.deltaTime * 20);
				showTimer -= Time.deltaTime;
			}
			else
			{
				panel.alpha = Mathf.Lerp(panel.alpha, 0, Time.deltaTime * 20);
			}
		}

		public void Show(string text) {
			label.text = text;
			showTimer = 5;
		}
	}
}
