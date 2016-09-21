using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Game Jolt API Helper Trophies methods.
/// </summary>
public class GJHTrophiesMethods
{
	/// <summary>
	/// The trophies window.
	/// </summary>
	GJHTrophiesWindow window;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHTrophiesMethods"/> class.
	/// </summary>
	public GJHTrophiesMethods ()
	{
		window = new GJHTrophiesWindow ();
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHTrophiesMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJHTrophiesMethods ()
	{
		window = null;
	}
		
	/// <summary>
	/// Sets the secret trophies.
	/// </summary>
	/// <param name='ids'>
	/// The trophies identifiers.
	/// </param>
	/// <param name='show'>
	/// <c>true</c> to show secret trophies with ???; <c>false</c> to not show secret trophies at all.
	/// </param>
	public void SetSecretTrophies (uint[] ids, bool show)
	{
		window.secretTrophies = ids;
		window.showSecretTrophies = show;
	}
	
	/// <summary>
	/// Shows the trophies window.
	/// </summary>
	public void ShowTrophies ()
	{
		window.Show ();
	}
	
	/// <summary>
	/// Dismisses the trophies window.
	/// </summary>
	public void DismissTrophies ()
	{
		window.Dismiss ();
	}
	
	/// <summary>
	/// Shows the trophy unlock notification.
	/// </summary>
	/// <param name='trophyID'>
	/// The trophy identifier.
	/// </param>
	public void ShowTrophyUnlockNotification (uint trophyID)
	{
		GJAPI.Trophies.GetOneCallback += OnGetTrophy;
		GJAPI.Trophies.Get (trophyID);
	}
	
	/// <summary>
	/// OnGetTrophy callback.
	/// </summary>
	/// <param name='trophy'>
	/// The trophy.
	/// </param>
	void OnGetTrophy (GJTrophy trophy)
	{
		GJAPI.Trophies.GetOneCallback -= OnGetTrophy;
		
		if (trophy != null)
		{
			DownloadTrophyIcon (trophy, tex =>
			{
				GJHNotification nofitication = new GJHNotification ( trophy.Title, trophy.Description, tex );
				GJHNotificationsManager.QueueNotification (nofitication);	
			});
		}
	}
	
	/// <summary>
	/// Downloads the trophy icon.
	/// </summary>
	/// <param name='trophy'>
	/// The trophy.
	/// </param>
	/// <param name='OnComplete'>
	/// The callback.
	/// </param>
	public void DownloadTrophyIcon (GJTrophy trophy, Action<Texture2D> OnComplete)
	{
		GJAPIHelper.DownloadImage (
			trophy.ImageURL,
			icon =>
			{
				if (icon == null)
				{
					icon = (Texture2D) Resources.Load ("Images/TrophyIcon", typeof (Texture2D)) ?? new Texture2D (75,75);	
				}
			
				if (OnComplete != null)
				{
					OnComplete (icon);
				}
			});
	}
}