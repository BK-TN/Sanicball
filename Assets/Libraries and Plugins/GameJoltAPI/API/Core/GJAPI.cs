using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

/// <summary>
/// Game Jolt API main class.
/// </summary>
public class GJAPI : MonoBehaviour
{
	#region Singleton Pattern
	/// <summary>
	/// The GJAPI instance.
	/// </summary>
	static GJAPI instance;
	/// <summary>
	/// Gets the GJAPI instance.
	/// </summary>
	/// <value>
	/// The GJAPI instance.
	/// </value>
	public static GJAPI Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject ("_GameJoltAPI").AddComponent<GJAPI> ();
				DontDestroyOnLoad (instance.gameObject);
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
		user = null;
		users = null;
		sessions = null;
		trophies = null;
		scores = null;
		data = null;
		instance = null;
			
		Debug.Log ("GJAPI: Quit");
	}
	#endregion Singleton Pattern
	
	#region GameJolt API Paths
	/// <summary>
	/// REST API paths.
	/// </summary>
	const string
		PROTOCOL = "http://",
		API_ROOT = "gamejolt.com/api/game/";
	#endregion GameJolt API Paths
	
	#region API Properties
	int gameId = 0;
	/// <summary>
	/// Gets the game identifier.
	/// </summary>
	/// <value>
	/// The game identifier.
	/// </value>
	public static int GameID
	{ 
		get { return Instance.gameId; }
	}
	
	string privateKey = string.Empty;
	/// <summary>
	/// Gets the game private key.
	/// </summary>
	/// <value>
	/// The game private key.
	/// </value>
	public static string PrivateKey
	{
		get { return Instance.privateKey; }
	}
	
	bool verbose = true;
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="GJAPI"/> is verbose.
	/// </summary>
	/// <value>
	/// <c>true</c> if verbose; otherwise, <c>false</c>.
	/// </value>
	public static bool Verbose
	{
		get { return Instance.verbose; }
		set { Instance.verbose = value; }
	}
	
	int version = 0;
	/// <summary>
	/// Gets or sets the API version.
	/// </summary>
	/// <value>
	/// The API version.
	/// </value>
	public static int Version
	{
		get { return Instance.version; }
		set { Instance.version = value; }
	}
	
	float timeout = 5f;
	/// <summary>
	/// Gets or sets the timeout length for API calls.
	/// </summary>
	/// <value>
	/// The timeout length.
	/// </value>
	public static float Timeout
	{
		get { return Instance.timeout; }
		set { Instance.timeout = value; }
	}
	#endregion API Properties
	
	#region API User
	GJUser user = null;
	/// <summary>
	/// Gets the user.
	/// </summary>
	/// <value>
	/// The user.
	/// </value>
	public static GJUser User
	{
		get { return Instance.user; }
		set { Instance.user = value; }
	}
	#endregion API User
	
	#region API Groups
	private GJUsersMethods users;
	/// <summary>
	/// Gets the users methods group.
	/// </summary>
	/// <value>
	/// The users methods group.
	/// </value>
	public static GJUsersMethods Users
	{
		get
		{
			if (Instance.users == null)
			{
				Instance.users = new GJUsersMethods ();
			}
			
			return Instance.users;
		}
	}

	private GJSessionsMethods sessions;
	/// <summary>
	/// Gets the sessions methods group.
	/// </summary>
	/// <value>
	/// The sessions methods group.
	/// </value>
	public static GJSessionsMethods Sessions
	{
		get
		{
			if (Instance.sessions == null)
			{
				Instance.sessions = new GJSessionsMethods ();
			}
			
			return Instance.sessions;
		}
	}

	private GJTrophiesMethods trophies;
	/// <summary>
	/// Gets the trophies methods group.
	/// </summary>
	/// <value>
	/// The trophies methods group.
	/// </value>
	public static GJTrophiesMethods Trophies
	{
		get
		{
			if (Instance.trophies == null)
			{
				Instance.trophies = new GJTrophiesMethods ();
			}
			
			return Instance.trophies;
		}
	}

	private GJScoresMethods scores;
	/// <summary>
	/// Gets the scores methods group.
	/// </summary>
	/// <value>
	/// The scores methods group.
	/// </value>
	public static GJScoresMethods Scores
	{
		get
		{
			if (Instance.scores == null)
			{
				Instance.scores = new GJScoresMethods ();
			}
			
			return Instance.scores;
		}
	}

	private GJDataMehods data;
	/// <summary>
	/// Gets the data methods group.
	/// </summary>
	/// <value>
	/// The data methods group.
	/// </value>
	public static GJDataMehods Data
	{
		get
		{
			if (Instance.data == null)
			{
				Instance.data = new GJDataMehods ();
			}
			
			return Instance.data;
		}
	}
	#endregion API Groups
	
	#region General
	// <summary>
	/// Init the GJAPI with the specified gameId, privateKey, verbose and version.
	/// </summary>
	/// <param name='gameId'>
	/// The Game ID.
	/// </param>
	/// <param name='privateKey'>
	/// The Game Private Key.
	/// </param>
	/// <param name='verbose'>
	/// A value indicating whether this <see cref="GJAPI"/> is verbose. Default true.
	/// </param>
	/// <param name='version'>
	/// The API version. Default 1.
	/// </param>
	public static void Init (int gameId, string privateKey, bool verbose = true, int version = 1)
	{		
		Instance.gameId = gameId;
		Instance.privateKey = privateKey.Trim ();
		Instance.verbose = verbose;
		Instance.version = version;
		Instance.user = null;
		
		Instance.GJDebug ("Initialisation complete.\n" + Instance.ToString());
	}
	
	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="GJAPI"/>.
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/> that represents the current <see cref="GJAPI"/>.
	/// </returns>
	public override string ToString ()
	{
		StringBuilder msg = new StringBuilder();
		msg.AppendLine (" [GJAPI]");
		msg.AppendFormat ("Game ID: {0}\n", Instance.gameId.ToString ());
#if UNITY_EDITOR
		msg.AppendFormat ("Private Key: {0}\n", Instance.privateKey);
#else
		msg.Append ("Private Key: [FILTERED]\n");
#endif
		msg.AppendFormat ("Verbose: {0}\n", Instance.verbose.ToString ());
		msg.AppendFormat ("Version: {0}\n", Instance.version.ToString ());
		return msg.ToString ();
	}
	#endregion General
	
	// Most of Internal Methods need to be public so API groups can access them.
	// However, they are not static so it is needed to do GJAPI.Instance.MethodName instead of GJAPI.MethodName
	// It may help users not to get confused with methods they shouldn't call.
	#region Internal Methods
	/// <summary>
	/// Send a request to the Game Jolt REST API.
	/// </summary>
	/// <param name='method'>
	/// The request method.
	/// </param>
	/// <param name='parameters'>
	/// The parameters.
	/// </param>
	/// <param name='requireVerified'>
	/// A value indicating whether a verified user is required.
	/// </param>
	/// <param name='OnResponseComplete'>
	/// The method to call when the request is completed.
	/// </param>
	public void Request (string method, Dictionary<string,string> parameters, bool requireVerified = false, Action<string> OnResponseComplete = null)
	{
		if (gameId == 0 || privateKey == string.Empty || version == 0)
		{
			GJDebug ("Please initialise GameJolt API first.", LogType.Error);
			if (OnResponseComplete != null)
			{
				OnResponseComplete ("Error:\nAPI needs to be initialised first.");
			}
			return;
		}
		
		if (parameters == null)
		{
			parameters = new Dictionary<string, string>();
		}
		
		if (requireVerified)
		{
			if (this.user == null)
			{
				GJDebug ("Authentification required for " + method, LogType.Error);
				if (OnResponseComplete != null)
				{
					OnResponseComplete ("Error:\nThe method " + method + " requires authentification.");
				}
				return;
			}
			else
			{
				parameters.Add ("username", this.user.Name);
				parameters.Add ("user_token", this.user.Token);
			}
		}
		
		string url = GetRequestURL (method, parameters);
		StartCoroutine (OpenURLAndGetResponse (url, OnResponseComplete));
	}
	
	/// /// <summary>
	/// Send a request to the Game Jolt REST API.
	/// </summary>
	/// <param name='method'>
	/// The request method.
	/// </param>
	/// <param name='parameters'>
	/// The parameters.
	/// </param>
	/// </param>
	/// <param name='postParameters'>
	/// The POST parameters.
	/// </param>
	/// <param name='requireVerified'>
	/// A value indicating whether a verified user is required.
	/// </param>
	/// <param name='OnResponseComplete'>
	/// The method to call when the request is completed.
	/// </param>
	public void Request (string method, Dictionary<string,string> parameters, Dictionary<string,string> postParameters, bool requireVerified = false, Action<string> OnResponseComplete = null)
	{
		if (gameId == 0 || privateKey == string.Empty || version == 0)
		{
			GJDebug ("Please initialise GameJolt API first.", LogType.Error);
			if (OnResponseComplete != null)
			{
				OnResponseComplete ("Error:\nAPI needs to be initialised first.");
			}
			return;
		}
		
		if (parameters == null)
		{
			parameters = new Dictionary<string, string>();
		}
		
		if (requireVerified)
		{
			if (this.user == null)
			{
				GJDebug ("Authentification required for " + method, LogType.Error);
				if (OnResponseComplete != null)
				{
					OnResponseComplete ("Error:\nThe method " + method + " requires authentification.");
				}
				return;
			}
			else
			{
				parameters.Add ("username", this.user.Name);
				parameters.Add ("user_token", this.user.Token);
			}
		}
		
		string url = GetRequestURL (method, parameters);
		StartCoroutine (OpenURLAndGetResponse (url, postParameters, OnResponseComplete));
	}
	
	/// <summary>
	/// Gets the formated URL for the method with the parameters.
	/// </summary>
	/// <returns>
	/// The request URL.
	/// </returns>
	/// <param name='method'>
	/// The method.
	/// </param>
	/// <param name='parameters'>
	/// The parameters.
	/// </param>
	string GetRequestURL (string method, Dictionary<string,string> parameters)
	{
		StringBuilder url = new StringBuilder ();
		url.Append (PROTOCOL);
		url.Append (API_ROOT);
		url.Append ("v");
		url.Append (this.version);
		url.Append ("/");
		url.Append (method);
		url.Append ("?game_id=");
		url.Append (this.gameId);
		
		foreach (KeyValuePair<string,string> parameter in parameters)
		{
			url.Append ("&");
			url.Append (parameter.Key);
			url.Append ("=");
			url.Append (parameter.Value.Replace (" ", "%20"));
		}
		
		string signature = GetSignature (url.ToString ());
		url.Append ("&signature=");
		url.Append (signature);
				
		return url.ToString();
	}
	
	/// <summary>
	/// Gets the request signature.
	/// </summary>
	/// <returns>
	/// The signature.
	/// </returns>
	/// <param name='input'>
	/// The base request URL.
	/// </param>
	string GetSignature (string input)
	{
		string signature = MD5 (input + this.privateKey);
		
		// Append zeroes (0) if the signature isn't 32 characters long.
		if (signature.Length != 32)
		{
			signature += new string ('0', 32 - signature.Length);
		}

		return signature;
	}
	
	/// <summary>
	/// Encrypt the input string to MD5.
	/// </summary>
	/// <returns>
	/// The encrypted string to MD5.
	/// </returns>
	/// <param name='input'>
	/// The string to encrypt.
	/// </param>
	/// <remarks>
	/// This method is taken from the first version of the Unity Game Jolt API and was written by Ashley Gwinnell and Daniel Twomey.
	/// </remarks>
	string MD5 (string input)
	{
		// WP8 and Windows Metro fix kindly provided by runewake2 [http://gamejolt.com/profile/runewake2/2008/]
#if UNITY_WP8 || UNITY_METRO
		byte[] data = MD5Core.GetHash(input, System.Text.Encoding.ASCII);
#else
		System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider ();
        byte [] data = System.Text.Encoding.ASCII.GetBytes (input);
        data = x.ComputeHash (data);
#endif
        string ret = "";
        for (int i=0; i < data.Length; i++)
			ret += data [i].ToString("x2").ToLower();		
        return ret;
	}
	
	/// <summary>
	/// Opens the URL wait for the response.
	/// </summary>
	/// <returns>
	/// The response.
	/// </returns>
	/// <param name='url'>
	/// The URL to open.
	/// </param>
	/// <param name='OnResponseComplete'>
	/// The method to call when the response is recieved.
	/// </param>
	IEnumerator OpenURLAndGetResponse (string url, Action<string> OnResponseComplete = null)
	{		
		GJDebug ("Opening URL: " + url);
		WWW www = new WWW (url);
		
		float callTimeout = Time.time + timeout;
		string msg = null;
		
		while (!www.isDone)
		{
			if (Time.time > callTimeout)
			{
				GJDebug ("Timeout opening URL:\n" + url, LogType.Error);
				msg = "Timeout";
				break;
			}
			yield return new WaitForEndOfFrame ();
		}
		
		if (www.error != null)
		{
			GJDebug ("Error opening URL:\n" + www.error, LogType.Error);
			msg = www.error;
		}
				
		if (OnResponseComplete != null)
		{
			// If msg is not null, it means something went wrong.
			// Thus, we shouldn't read www.text because it's not ready.
			OnResponseComplete (msg ?? www.text);
		}
	}
	
	/// <summary>
	/// Opens the URL wait for the response.
	/// </summary>
	/// <returns>
	/// The response.
	/// </returns>
	/// <param name='url'>
	/// The URL to open.
	/// </param>
	/// <param name='postParameters'>
	/// The POST parameters.
	/// </param>
	/// <param name='OnResponseComplete'>
	/// The method to call when the response is recieved.
	/// </param>
	IEnumerator OpenURLAndGetResponse (string url, Dictionary<string,string> postParameters, Action<string> OnResponseComplete = null)
	{
		StringBuilder debugMsg = new StringBuilder ();
		debugMsg.AppendFormat ("Opening URL with post parameters: {0}\n", url);
		
		if (postParameters == null || postParameters.Count == 0)
		{
			GJDebug ("Post parameters is null. Can't make the request.", LogType.Error);
			yield break;
		}
				
		WWWForm form = new WWWForm ();
		foreach (KeyValuePair<string,string> postParameter in postParameters)
		{
			debugMsg.AppendFormat ("Post parameter: {0}: {1}\n", postParameter.Key, postParameter.Value);
			form.AddField (postParameter.Key, postParameter.Value);
		}
		
		
		GJDebug (debugMsg.ToString ());
		WWW www = new WWW (url, form);
		
		float callTimeout = Time.time + 5f;
		string msg = null;
		
		while (!www.isDone)
		{
			if (Time.time > callTimeout)
			{
				GJDebug ("Timeout opening URL:\n" + url, LogType.Error);
				msg = "Timeout";
				break;
			}
			yield return new WaitForEndOfFrame ();
		}
		
		if (www.error != null)
		{
			GJDebug ("Error opening URL:\n" + www.error, LogType.Error);
			msg = www.error;
		}
				
		if (OnResponseComplete != null)
		{
			// If msg is not null, it means something went wrong.
			// Thus, we shouldn't read www.text because it's not ready.
			OnResponseComplete (msg ?? www.text);
		}
	}
	
	/// <summary>
	/// Determines whether the response is successful.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the response is successful; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='response'>
	/// The response.
	/// </param>
	public bool IsResponseSuccessful (string response)
	{
		string [] lines = response.Split ('\n');
		return lines [0].Trim().Equals ("success:\"true\"");
	}
	
	/// <summary>
	/// Determines whether the dump response is successful.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the dump response is successful; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='response'>
	/// The dump response. Because dump response can be up to 16 MB, it is passed as a reference. However, the method won't modify it.
	/// </param>
	public bool IsDumpResponseSuccessful (ref string response)
	{
		int returnIndex = response.IndexOf ('\n');
		if (returnIndex == -1)
		{
			GJDebug ("Wrong response format. Can't read response.", LogType.Error);
			return false;
		}
		else
		{
			string success = response.Substring (0, returnIndex).Trim ();		
			return success == "SUCCESS";	
		}
	}
		
	/// <summary>
	/// Converts the responses to a dictionary.
	/// </summary>
	/// <returns>
	/// The dictionary.
	/// </returns>
	/// <param name='response'>
	/// The response.
	/// </param>
	/// <param name='addIndexToKey'>
	/// Add index to key. Set to true if the response is expected to have duplicated keys.
	/// </param>
	public Dictionary<string, string> ResponseToDictionary (string response, bool addIndexToKey = false)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		int colonIndex = 0;
		string key = string.Empty;
		string val = string.Empty;
		
		string [] lines = response.Split ('\n');
		int count = lines.Length;
		for (int i = 0; i < count; i++)
		{
			if (lines [i] != string.Empty)
			{
				// The format of each line of the response should be key:"value"
				colonIndex = lines [i].IndexOf (':');
				if (colonIndex == -1)
				{
					GJDebug ("Wrong line format. The following line of the response will be skipped: " + lines [i], LogType.Warning);
				}
				else
				{
					key = lines [i].Substring (0, colonIndex);
					val = lines [i].Substring (colonIndex + 1);
					val = val.Trim().Trim ('"');
					
					if (addIndexToKey)
					{
						dictionary.Add (key + i, val);
					}
					else
					{
						dictionary.Add (key, val);	
					}
				}
			}
		}
		
		return dictionary;
	}
	
	/// <summary>
	/// Converts the responses to a dictionaries.
	/// </summary>
	/// <returns>
	/// The dictionaries.
	/// </returns>
	/// <param name='response'>
	/// The response.
	/// </param>
	/// <param name='addIndexToKey'>
	/// Add index to key. Set to true if the response is expected to have duplicated keys.
	/// </param>
	public Dictionary<string,string> [] ResponseToDictionaries (string response, bool addIndexToKey = false)
	{
		// Using json/xml responses when retriving multiple object would be less cpu expensive
		// but we want to avoid the need of adding heavy libraries (especially for browser game).
		int colonIndex = 0;
		int numberOfObjects = 0;
		int dictionaryIndex = 0;
		string key = string.Empty;
		string val = string.Empty;
		string firstKey = string.Empty;
		
		string [] lines = response.Split ('\n');
		int count = lines.Length;
		
		// First, we need to know how many objects got returned.
		for (int i = 0; i < count; i++)
		{
			if (lines [i] != string.Empty)
			{
				colonIndex = lines [i].IndexOf (':');
				if (colonIndex != -1)
				{
					key = lines [i].Substring (0, colonIndex);
					
					if (key != "success" && key != "message")
					{
						if (firstKey == string.Empty)
						{
							firstKey = key;
							numberOfObjects++;
						}
						else if (firstKey == key)
						{
							numberOfObjects++;
						}
					}
				}
			}
		}
		firstKey = string.Empty; // Reset
		
		// Now, we can initialise the right number of dictionaries.
		Dictionary<string,string> [] dictionaries = new Dictionary<string, string> [numberOfObjects];
		for (int i = 0; i < numberOfObjects; i++)
		{
			dictionaries [i] = new Dictionary<string,string>();
		}
		
		// Finaly, we can populate them.
		for (int i = 0; i < count; i++)
		{
			if (lines [i] != string.Empty)
			{
				colonIndex = lines [i].IndexOf (':');
				if (colonIndex == 1)
				{
					GJDebug ("Wrong line format. The following line of the response will be skipped: " + lines [i], LogType.Warning);
				}
				else
				{
					key = lines [i].Substring (0, colonIndex);
					val = lines [i].Substring (colonIndex + 1);
					val = val.Trim().Trim ('"');
					
					if (key != "success" && key != "message")
					{
						if (firstKey == string.Empty)
						{
							firstKey = key;
						}
						else if (firstKey == key)
						{
							dictionaryIndex++;
						}
					}
					
					if (addIndexToKey)
					{
						dictionaries [dictionaryIndex].Add (key + i, val);
					}
					else
					{
						dictionaries [dictionaryIndex].Add (key, val);	
					}
				}
			}
		}
		
		return dictionaries;
	}
	
	/// <summary>
	/// Remove unwanted keys from the dictionary.
	/// </summary>
	/// <param name='dictionary'>
	/// The cleaned dictionary.
	/// </param>
	/// <param name='keysToClean'>
	/// The keys to remove from the dictionary. By default, "success" and "message".
	/// </param>
	public void CleanDictionary (ref Dictionary<string, string> dictionary, string [] keysToClean = null)
	{
		// We can't use a populated array as optional parameter unless it's null. We then populate the array in the method.
		if (keysToClean == null)
		{
			keysToClean = new string [] { "success", "message" };
		}
		
		int count = keysToClean.Length;
		for (int i = 0; i < count; i++)
		{
			if (dictionary.ContainsKey (keysToClean [i]))
			{
				dictionary.Remove (keysToClean [i]);
			}	
		}
	}
	
	/// <summary>
	/// Remove unwanted keys from the dictionaries.
	/// </summary>
	/// <param name='dictionary'>
	/// The cleaned dictionaries.
	/// </param>
	/// <param name='keysToClean'>
	/// The keys to remove from the dictionaries. By default, "success" and "message".
	/// </param>
	public void CleanDictionaries (ref Dictionary<string,string> [] dictionaries, string [] keysToClean = null)
	{
		int count = dictionaries.Length;
		for (int i = 0; i < count; i++)
		{
			CleanDictionary (ref dictionaries [i], keysToClean);
		}
	}
	
	/// <summary>
	/// Converts the dump response to string.
	/// </summary>
	/// <param name='response'>
	/// The dump response. Because dump response can be up to 16 MB, it is passed as a reference. However, the method won't modify it.
	/// </param>
	/// <param name='data'>
	/// The dumpt data. Because the data can be up to 16 MB, it is passed as out.
	/// </param>
	public void DumpResponseToString (ref string response, out string data)
	{
		int returnIndex = response.IndexOf ('\n');
		if (returnIndex == -1)
		{
			GJDebug ("Wrong response format. Can't read response.", LogType.Error);
			data = string.Empty;
		}
		else
		{
			data = response.Substring (returnIndex + 1);	
		}
	}
	
	/// <summary>
	/// Print debug information to the console only if the API is verbose.
	/// </summary>
	/// <param name='message'>
	/// The message.
	/// </param>
	/// <param name='type'>
	/// The message type. See <see cref="UnityEngine.LogType"/>.
	/// </param>
	public void GJDebug (string message, LogType type = LogType.Log)
	{
		if (!verbose)
		{
			return;
		}
		
		switch (type)
		{
		case LogType.Log:
		default:
			Debug.Log ("GJAPI: " + message);
			break;
		case LogType.Warning:
			Debug.LogWarning ("GJAPI: " + message);
			break;
		case LogType.Error:
			Debug.LogError ("GJAPI: " + message);
			break;
		}
	}
	#endregion Internal Methods
}