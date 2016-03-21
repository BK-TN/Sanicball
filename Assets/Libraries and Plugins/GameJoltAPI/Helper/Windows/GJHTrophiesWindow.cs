using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Helper Trophies Window. Inherit from <see cref="GJHWindow"/>
/// </summary>
public class GJHTrophiesWindow : GJHWindow
{
	/// <summary>
	/// The trophies scroll view position.
	/// </summary>
	Vector2 trophiesScrollViewPosition;
	/// <summary>
	/// The trophies.
	/// </summary>
	GJTrophy[] trophies = null;
	/// <summary>
	/// The trophies icons.
	/// </summary>
	Texture2D[] trophiesIcons = null;
	
	/// <summary>
	/// The window states.
	/// </summary>
	enum TrophiesWindowStates { TrophiesList }
	
	/// <summary>
	/// GUI styles.
	/// </summary>
	GUIStyle
		trophyTitleStyle = null,
		trophyDescriptionStyle = null;
	
	/// <summary>
	/// The secret trophies ids.
	/// </summary>
	public uint[] secretTrophies = null;
	/// <summary>
	/// <c>true</c> to show secret trophies with ???; <c>false</c> to not show secret trophies at all.
	/// </summary>
	public bool showSecretTrophies = true;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHTrophiesMethodsWindow"/> class.
	/// </summary>
	public GJHTrophiesWindow () : base ()
	{
		Title = "Trophies";
		float w = Screen.width * .9f;
		w = w > 500 ? 500 : w;
		float h = Screen.height * .9f;
		Position = new Rect (Screen.width / 2 - w / 2, Screen.height / 2 - h / 2, w, h);
		
		drawWindowDelegates.Add (TrophiesWindowStates.TrophiesList.ToString (), DrawTrophiesList);
		
		trophyTitleStyle = GJAPIHelper.Skin.FindStyle ("TrophyTitle") ?? GJAPIHelper.Skin.label;
		trophyDescriptionStyle = GJAPIHelper.Skin.FindStyle ("TrophyDescription") ?? GJAPIHelper.Skin.label;
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHTrophiesWindow"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJHTrophiesWindow ()
	{
		trophies = null;
		trophiesIcons = null;
		trophyTitleStyle = null;
		trophyDescriptionStyle = null;
		secretTrophies = null;
	}
	
	/// <summary>
	/// Show this window.
	/// </summary>
	public override bool Show ()
	{
		bool success = base.Show ();
		if (success)
		{
			GetTrophies ();
		}
		return success;
	}
	
	/// <summary>
	/// Dismiss this window.
	/// </summary>
	public override bool Dismiss ()
	{
		return base.Dismiss ();
	}
	
	/// <summary>
	/// Gets the trophies.
	/// </summary>
	void GetTrophies ()
	{
		SetWindowMessage ("Loading trophies");
		ChangeState (BaseWindowStates.Process.ToString ());
		
		GJAPI.Trophies.GetAllCallback += OnGetTrophies;
		GJAPI.Trophies.GetAll ();
	}
	
	/// <summary>
	/// GetTrophies callback.
	/// </summary>
	/// <param name='t'>
	/// The trophies.
	/// </param>
	void OnGetTrophies (GJTrophy[] t)
	{
		GJAPI.Trophies.GetAllCallback -= OnGetTrophies;
		
		if (t == null)
		{
			SetWindowMessage ("Error loading trophies.");
			ChangeState (BaseWindowStates.Error.ToString ());
			return;
		}
		
		trophies = t;
		
		int count = trophies.Length;
		trophiesIcons = new Texture2D[count];
		for (int i = 0; i < count; i++)
		{
			trophiesIcons[i] = (Texture2D) Resources.Load ("Images/TrophyIcon", typeof (Texture2D)) ?? new Texture2D (75,75);
			int index = i; // If we pass i directly, it passes a reference and will be out of range.
			GJAPIHelper.Trophies.DownloadTrophyIcon (
				trophies[i],
				icon => { trophiesIcons[index] = icon; });
		}
		
		ChangeState (TrophiesWindowStates.TrophiesList.ToString ());
	}
	
	/// <summary>
	/// Draws the trophies list.
	/// </summary>
	void DrawTrophiesList ()
	{
		trophiesScrollViewPosition = GUILayout.BeginScrollView (trophiesScrollViewPosition);
		int count = trophies.Length;
		for (int i = 0 ; i < count ; i++)
		{	
			if (secretTrophies != null
				&& secretTrophies.Length > 0
				&& ((IList<uint>)secretTrophies).Contains(trophies[i].Id)
				&& !trophies[i].Achieved)
			{
				if (!showSecretTrophies)
				{
					continue;
				}
				DrawTrophy (i, false);
			}
			else
			{
				DrawTrophy (i, true);
			}
			
			if (i != count - 1)
			{
				GUILayout.Space (10);
			}
		}
		GUILayout.EndScrollView ();
		
		GUILayout.Space (10);
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Close"))
		{
			Dismiss ();
		}
		GUILayout.EndHorizontal ();
	}
	
	/// <summary>
	/// Draws the trophy.
	/// </summary>
	/// <param name='t'>
	/// The trophy.
	/// </param>
	void DrawTrophy (int t, bool show)
	{
		GUILayout.BeginHorizontal ();
		GUI.enabled = trophies[t].Achieved ? true : false;
		GUILayout.Label (trophiesIcons[t]);
		GUI.enabled = true;
		GUILayout.Space (10);
		GUILayout.BeginVertical ("box", GUILayout.Height (75));
		GUILayout.FlexibleSpace ();
		GUILayout.Label (show ? trophies[t].Title : "???", trophyTitleStyle);
		GUILayout.Space (5);
		GUILayout.Label (show ? trophies[t].Description : "???", trophyDescriptionStyle);
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
	}
}
