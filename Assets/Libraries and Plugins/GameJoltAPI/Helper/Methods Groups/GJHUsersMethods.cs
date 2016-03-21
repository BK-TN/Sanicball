using UnityEngine;
using System;

/// <summary>
/// Game Jolt API Helper User methods.
/// </summary>
public class GJHUsersMethods
{
	/// <summary>
	/// The getFromWeb callback.
	/// </summary>
	Action <string,string> getFromWebCallback = null;
	/// <summary>
	/// The login window.
	/// </summary>
	GJHLoginWindow window = null;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHUsersMethods"/> class.
	/// </summary>
	public GJHUsersMethods ()
	{
		window = new GJHLoginWindow ();
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHUsersMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJHUsersMethods ()
	{
		getFromWebCallback = null;
		window = null;
	}
	
	/// <summary>
	/// Gets user informations from web.
	/// </summary>
	/// <param name='onComplete'>
	/// The callback.
	/// </param>
	public void GetFromWeb (Action<string,string> onComplete)
	{
		#if UNITY_WEBPLAYER
		getFromWebCallback = onComplete;
		Application.ExternalCall ("GJAPI_AuthUser", GJAPIHelper.Instance.gameObject.name, "OnGetUserFromWeb");
		#else
		getFromWebCallback = null;
		if (onComplete != null)
		{
			onComplete (string.Empty, string.Empty);
		}
		Debug.Log ("GJAPIHelper: The method \"GetFromWeb\" can only be called from WebPlayer builds.");
		#endif
	}
	
	/// <summary>
	/// Reads getFromWeb response.
	/// </summary>
	/// <param name='response'>
	/// Rhe response.
	/// </param>
	public void ReadGetFromWebResponse (string response)
	{
		if (getFromWebCallback == null)
			return;
		
		string name = string.Empty;
		string token = string.Empty;
		
		if (response.ToLower() == "false" || response == string.Empty || response.ToLower() == "Guest")
		{
			name = "Guest";
			token =  string.Empty;
		}
		else
		{
			string[] splittedResponse = response.Split (':');
			name = splittedResponse[0];
    		token = splittedResponse[1];
		}

		getFromWebCallback (name, token);
	}
	
	/// <summary>
	/// Show some love to the user.
	/// </summary>
	public void ShowGreetingNotification ()
	{
		if (GJAPI.User == null)
		{
			Debug.LogWarning ("GJAPIHelper: There is no verified user to show greetings to ;-(");
			return;
		}
		
		GJHNotification notification = new GJHNotification (string.Format ("Welcome back {0}!", GJAPI.User.Name));
		GJHNotificationsManager.QueueNotification (notification);
	}
	
	/// <summary>
	/// Shows the login window.
	/// </summary>
	public void ShowLogin ()
	{
		window.Show ();
	}
	
	/// <summary>
	/// Dismisses the login window.
	/// </summary>
	public void DismissLogin ()
	{
		window.Dismiss ();
	}
	
	/// <summary>
	/// Downloads the user avatar.
	/// </summary>
	/// <param name='user'>
	/// The user.
	/// </param>
	/// <param name='OnComplete'>
	/// The callback.
	/// </param>
	public void DownloadUserAvatar (GJUser user, Action<Texture2D> OnComplete)
	{
		GJAPIHelper.DownloadImage (
			user.AvatarURL,
			avatar =>
			{
				if (avatar == null)
				{
					avatar = (Texture2D) Resources.Load ("Images/UserAvatar", typeof (Texture2D)) ?? new Texture2D (60,60);	
				}
			
				if (OnComplete != null)
				{
					OnComplete (avatar);
				}
			});
	}
}