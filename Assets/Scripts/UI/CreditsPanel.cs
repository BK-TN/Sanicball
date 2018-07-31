using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;

namespace Sanicball.UI {
	public class CreditsPanel : MonoBehaviour {

		public Text characterList;
		public Text trackList;
		public MusicPlayer musicPlayerPrefab;

		void Start () {
			var characterText = new List<string> ();
			var characters = ActiveData.Characters;
			foreach (Sanicball.Data.CharacterInfo c in characters.Where(a => !a.hidden).OrderBy(a => a.tier)) {
				characterText.Add (c.name + ": <b>" + c.artBy + "</b>");
			}
			characterList.text = string.Join ("\n", characterText.ToArray ());

			var tracksText = new List<string> ();
			var tracks = musicPlayerPrefab.playlist;
			foreach (Song s in tracks) 
			{
				tracksText.Add ("<b>" + s.name + "</b>");
			}
			trackList.text = string.Join ("\n", tracksText.ToArray ());
		}
		
		void Update () {
		
		}
	}
}