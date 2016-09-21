using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Game Jolt API Helper main class. Inherit from <see cref="MonoBehaviour"/>
/// </summary>
public class GJAPIHelper : MonoBehaviour {

	#region Singleton Pattern
	/// <summary>
	/// The GJAPIHelper instance.
	/// </summary>
	private static GJAPIHelper instance;
	/// <summary>
	/// Gets the GJAPIHelper instance.
	/// </summary>
	/// <value>
	/// The GJAPIHelper instance.
	/// </value>
	public static GJAPIHelper Instance
	{
		get
		{
			if (instance == null)
			{
				GJAPI gjapi = (GJAPI) FindObjectOfType (typeof (GJAPI));
				
				if (gjapi == null)
				{
					Debug.LogError ("An instance of GJAPI is needed in the scene, but there is none. Can't initialise GJAPIHelper.");
				}
				else
				{
					instance = gjapi.gameObject.AddComponent<GJAPIHelper>();
					
					if (instance == null)
					{
						Debug.Log ("An error occured creating GJAPIHelper.");
					}
				}
			}
 
			return instance;
		}
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the application quit.
	/// </summary>
	void OnDestroy ()
	{
		StopAllCoroutines ();
		skin = null;
		users = null;
		scores = null;
		trophies = null;
		instance = null;
	}
	#endregion Singleton Pattern
	
	/// <summary>
	/// The <see cref="GUISkin"/>.
	/// </summary>
	protected GUISkin skin = null;
	/// <summary>
	/// Gets or sets the <see cref="GUISkin"/>.
	/// </summary>
	/// <value>
	/// The <see cref="GUISkin"/>.
	/// </value>
	public static GUISkin Skin
	{
		get
		{
			if (Instance.skin == null) {
				Instance.skin = (GUISkin) Resources.Load ("GJSkin", typeof (GUISkin)) ?? GUI.skin;
			}
			
			return Instance.skin;
		}
		set { Instance.skin = value; }
	}
	
	/// <summary>
	/// The users helpers.
	/// </summary>
	GJHUsersMethods users = null;
	/// <summary>
	/// Gets the users helpers.
	/// </summary>
	/// <value>
	/// The users helpers.
	/// </value>
	public static GJHUsersMethods Users
	{
		get
		{
			if (Instance.users == null)
			{
				Instance.users = new GJHUsersMethods ();
			}
			
			return Instance.users;
		}
	}
	
	/// <summary>
	/// The scores helpers.
	/// </summary>
	GJHScoresMethods scores = null;
	/// <summary>
	/// Gets the scores helpers.
	/// </summary>
	/// <value>
	/// The scores helpers.
	/// </value>
	public static GJHScoresMethods Scores
	{
		get
		{
			if (Instance.scores == null)
			{
				Instance.scores = new GJHScoresMethods ();
			}
			
			return Instance.scores;
		}
	}
	
	/// <summary>
	/// The trophies helpers.
	/// </summary>
	GJHTrophiesMethods trophies = null;
	/// <summary>
	/// Gets the trophies helpers.
	/// </summary>
	/// <value>
	/// The trophies helpers.
	/// </value>
	public static GJHTrophiesMethods Trophies
	{
		get
		{
			if (Instance.trophies == null)
			{
				Instance.trophies = new GJHTrophiesMethods ();
			}
			
			return Instance.trophies;
		}
	}
	
	/// <summary>
	/// Downloads the image.
	/// </summary>
	/// <param name='url'>
	/// The image URL.
	/// </param>
	/// <param name='OnComplete'>
	/// The callback.
	/// </param>
	public static void DownloadImage (string url, Action<Texture2D> OnComplete)
	{
		Instance.StartCoroutine (Instance.DownloadImageCoroutine (url, OnComplete));
	}
	
	/// <summary>
	/// Downloads the image coroutine.
	/// </summary>
	/// <param name='url'>
	/// The image URL.
	/// </param>
	/// <param name='OnComplete'>
	/// The callback.
	/// </param>
	IEnumerator DownloadImageCoroutine (string url, Action<Texture2D> OnComplete)
	{
		if (!string.IsNullOrEmpty (url))
		{
			Texture2D tex;
			WWW www = new WWW (url);
			yield return www;
			
			if (www.error == null)
			{
				tex = new Texture2D (1, 1, TextureFormat.RGB24, false);
				tex.LoadImage (www.bytes);
				tex.wrapMode = TextureWrapMode.Clamp;
			}
			else
			{
				Debug.Log ("GJAPIHelper: Error downloading image:\n" + www.error);
				tex = null;
			}
			
			if (OnComplete != null)
			{
				OnComplete (tex);
			}
		}
	}
	
	public void OnGetUserFromWeb (string response)
	{
		users.ReadGetFromWebResponse (response);
	}
}