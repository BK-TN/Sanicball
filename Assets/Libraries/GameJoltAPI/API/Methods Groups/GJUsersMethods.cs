using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Game Jolt API Users mehods.
/// </summary>
public class GJUsersMethods
{
	const string
		USERS_AUTH = "users/auth/",
		USERS_FETCH = "users/";
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJUsersMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJUsersMethods ()
	{
		VerifyCallback = null;
		GetVerifiedCallback = null;
		GetOneCallback = null;
		GetMultipleCallback = null;
	}
	
	public delegate void _VerifyCallback (bool verified);
	/// <summary>
	/// The verify user callback.
	/// </summary>
	public _VerifyCallback VerifyCallback = null;
	
	/// <summary>
	/// Verify the specified user.
	/// </summary>
	/// <param name='name'>
	/// The username.
	/// </param>
	/// <param name='token'>
	/// The user token.
	/// </param>
	public void Verify (string name, string token)
	{
		if (name.Trim () == string.Empty || token.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Either name or token is empty. Can't verify user.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Verifying user.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add ("username", name);
		parameters.Add ("user_token", token);
		
		GJAPI.User = new GJUser ();
		GJAPI.User.Name = name;
		GJAPI.User.Token = token;
		
		GJAPI.Instance.Request (USERS_AUTH, parameters, false, ReadVerifyResponse);
	}
	
	/// <summary>
	/// Reads the verify user response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadVerifyResponse (string response)
	{		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not verify the user.\n" + response, LogType.Error);
			GJAPI.User = null;
		}
		else
		{
			GJAPI.Instance.GJDebug ("User successfully verified.\n" + GJAPI.User.ToString());
		}
		
		if (VerifyCallback != null)
		{
			VerifyCallback (success);
		}
	}
	
	public delegate void _GetVerifiedCallback (GJUser user);
	/// <summary>
	/// The get verified user callback.
	/// </summary>
	public _GetVerifiedCallback GetVerifiedCallback = null;
	
	/// <summary>
	/// Gets the verified user.
	/// </summary>
	public void GetVerified ()
	{
		if (GJAPI.User == null)
		{
			GJAPI.Instance.GJDebug ("There is no verified user. Please verify a user before calling GetVerifiedUser.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting verified user.");
		
		// Because we required authentification, there is no need to pass username and user_token to the Request method, it will be added automatically.
		// And because those are the only two parameters needed, we can pass null.
		GJAPI.Instance.Request (USERS_FETCH, null, true, ReadGetVerifiedResponse);
	}
	
	/// <summary>
	/// Reads the get verified user response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetVerifiedResponse (string response)
	{		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the verified user.\n" + response, LogType.Error);
			GJAPI.User = null;
		}
		else
		{			
			Dictionary<string,string> dictionary = GJAPI.Instance.ResponseToDictionary (response);
			GJAPI.Instance.CleanDictionary (ref dictionary);
			
			GJAPI.User.AddProperties (dictionary, true);
			
			GJAPI.Instance.GJDebug ("Verified user successfully fetched.\n" + GJAPI.User.ToString());
		}
				
		if (GetVerifiedCallback != null)
		{
			GetVerifiedCallback (GJAPI.User);
		}
	}
	
	public delegate void _GetOneCallback (GJUser user);
	/// <summary>
	/// The get one user callback.
	/// </summary>
	public _GetOneCallback GetOneCallback = null;
	
	/// <summary>
	/// Get the specified user.
	/// </summary>
	/// <param name='name'>
	/// The user name.
	/// </param>
	public void Get (string name)
	{
		if (name.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Name is empty. Can't get user.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting user.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add ("username", name);
		
		GJAPI.Instance.Request (USERS_FETCH, parameters, false, ReadGetOneResponse);
	}
	
	/// <summary>
	/// Get the specified user.
	/// </summary>
	/// <param name='id'>
	/// The user identifier.
	/// </param>
	public void Get (uint id)
	{	
		GJAPI.Instance.GJDebug ("Getting user.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add ("user_id", id.ToString());
		
		GJAPI.Instance.Request (USERS_FETCH, parameters, false, ReadGetOneResponse);
	}
	
	/// <summary>
	/// Reads the get one user response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetOneResponse (string response)
	{		
		GJUser user;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the user.\n" + response, LogType.Error);
			user = null;
		}
		else
		{			
			Dictionary<string,string> dictionary = GJAPI.Instance.ResponseToDictionary (response);
			GJAPI.Instance.CleanDictionary (ref dictionary);
			
			user = new GJUser (dictionary);
			GJAPI.Instance.GJDebug ("User successfully fetched.\n" + user.ToString());
		}
		
		if (GetOneCallback != null)
		{
			GetOneCallback (user);
		}
	}
	
	public delegate void _GetMultipleCallback (GJUser[] users);
	/// <summary>
	/// The get multiple users callback.
	/// </summary>
	public _GetMultipleCallback GetMultipleCallback = null;
		
	/// <summary>
	/// Get the specified users.
	/// </summary>
	/// <param name='ids'>
	/// The users dentifiers.
	/// </param>
	public void Get (uint[] ids)
	{
		if (ids == null)
		{
			GJAPI.Instance.GJDebug ("IDs are null. Can't get users.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting users.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		string joinedIds = string.Join (",", new List<uint>(ids).ConvertAll (i => i.ToString ()).ToArray ());
		parameters.Add ("user_id", joinedIds);
		
		GJAPI.Instance.Request (USERS_FETCH, parameters, false, ReadGetMultipleResponse);
	}
	
	/// <summary>
	/// Reads the get multiple users response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetMultipleResponse (string response)
	{		
		GJUser[] users;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the users.\n" + response, LogType.Error);
			users = null;
		}
		else
		{			
			Dictionary<string,string>[] dictionaries = GJAPI.Instance.ResponseToDictionaries (response);
			GJAPI.Instance.CleanDictionaries (ref dictionaries);
			
			StringBuilder debug = new StringBuilder();
			debug.Append ("Users successfully fetched.\n");
			
			int count = dictionaries.Length;
			users = new GJUser [count];
			for (int i = 0; i < count; i++)
			{
				users [i] = new GJUser (dictionaries [i]);
				debug.Append (users [i].ToString ());
			}
			
			GJAPI.Instance.GJDebug (debug.ToString ());
		}
		
		if (GetMultipleCallback != null)
		{
			GetMultipleCallback (users);
		}
	}
}