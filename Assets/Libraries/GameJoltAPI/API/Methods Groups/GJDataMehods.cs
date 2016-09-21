using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Data mehods.
/// </summary>
public class GJDataMehods
{
	const string
		DATA_FETCH = "data-store/",
		DATA_SET = "data-store/set/",
		DATA_UPDATE = "data-store/update/",
		DATA_REMOVE = "data-store/remove/",
		DATA_KEYS = "data-store/get-keys/";
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJDataMehods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJDataMehods ()
	{
		SetCallback = null;
		UpdateSuccessCallback = null;
		UpdateCallback = null;
		GetCallback = null;
		RemoveKeyCallback = null;
		GetKeysCallback = null;
	}
	
	public delegate void _SetCallback (bool success);
	/// <summary>
	/// The set data callback.
	/// </summary>
	public _SetCallback SetCallback = null;
	
	/// <summary>
	/// Set the specified data.
	/// </summary>
	/// <param name='key'>
	/// The data key.
	/// </param>
	/// <param name='val'>
	/// The data value.
	/// </param>
	/// <param name='userData'>
	/// <c>true</c> if is user data; otherwise, <c>false</c>. Default false (i.e. game data).
	/// </param>
	public void Set (string key, string val, bool userData = false)
	{
		if (key.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Key is empty. Can't add data.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Adding data.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("key", key);
		
		Dictionary<string,string> postParameters = new Dictionary<string, string> ();
		postParameters.Add ("data", val);
		
		GJAPI.Instance.Request (DATA_SET, parameters, postParameters, userData, ReadSetResponse);
	}
	
	/// <summary>
	/// Reads the set data response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadSetResponse (string response)
	{
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not add data.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Data successfully added.");
		}
		
		if (SetCallback != null)
		{
			SetCallback (success);
		}
	}
	
	public enum UpdateOperation { Add, Subtract, Multiply, Divide, Append, Prepend };
	
	public delegate void _UpdateSuccessCallback (bool success);
	/// <summary>
	/// The update data success callback.
	/// </summary>
	public _UpdateSuccessCallback UpdateSuccessCallback = null;
	
	public delegate void _UpdateCallback (string data);
	/// <summary>
	/// The update data callback.
	/// </summary>
	public _UpdateCallback UpdateCallback = null;
	
	/// <summary>
	/// Update the specified data.
	/// </summary>
	/// <param name='key'>
	/// The data key.
	/// </param>
	/// <param name='val'>
	/// The data value.
	/// </param>
	/// <param name='operation'>
	/// The operation type.
	/// </param>
	/// <param name='userData'>
	/// <c>true</c> if is user data; otherwise, <c>false</c>. Default false (i.e. game data).
	/// </param>
	public void Update (string key, string val, UpdateOperation operation, bool userData = false )
	{
		if (key.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Key is empty. Can't get data.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Updating data.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("key", key);
		parameters.Add ("operation", operation.ToString ().ToLower ());
		parameters.Add ("format", "dump");
		
		Dictionary<string,string> postParameters = new Dictionary<string, string> ();
		postParameters.Add ("value", val);
		
		GJAPI.Instance.Request (DATA_UPDATE, parameters, postParameters, userData, ReadUpdateResponse);
	}
	
	/// <summary>
	/// Reads the update data response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadUpdateResponse (string response)
	{
		string data = string.Empty;
		
		bool success = GJAPI.Instance.IsDumpResponseSuccessful (ref response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not update data.\n" + response, LogType.Error);
			
		}
		else
		{
			GJAPI.Instance.DumpResponseToString (ref response, out data);
			
			if (data == string.Empty)
			{
				GJAPI.Instance.GJDebug ("Data successfully updated. However data is empty.", LogType.Warning);
			}
			else
			{
				GJAPI.Instance.GJDebug ("Data successfully updated.\n" + data);
			}
		}
		
		if (UpdateSuccessCallback != null)
		{
			UpdateSuccessCallback (success);
		}
		
		if (UpdateCallback != null)
		{
			UpdateCallback (data);
		}
	}
	
	public delegate void _GetCallback (string data);
	/// <summary>
	/// The get data callback.
	/// </summary>
	public _GetCallback GetCallback = null;
	
	/// <summary>
	/// Get the specified data.
	/// </summary>
	/// <param name='key'>
	/// The key.
	/// </param>
	/// <param name='userData'>
	/// <c>true</c> if is user data; otherwise, <c>false</c>. Default false (i.e. game data).
	/// </param>
	public void Get (string key, bool userData = false)
	{
		if (key.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Key is empty. Can't get data.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting data.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("key", key);
		parameters.Add ("format", "dump");
				
		GJAPI.Instance.Request (DATA_FETCH, parameters, userData, ReadGetResponse);
	}
	
	/// <summary>
	/// Reads the get data response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetResponse (string response)
	{
		string data = string.Empty;
		
		bool success = GJAPI.Instance.IsDumpResponseSuccessful (ref response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not fetch data.\n" + response, LogType.Error);
			
		}
		else
		{
			GJAPI.Instance.DumpResponseToString (ref response, out data);
			
			if (data == string.Empty)
			{
				GJAPI.Instance.GJDebug ("Data successfully fetched. However data is empty.", LogType.Warning);
			}
			else
			{
				GJAPI.Instance.GJDebug ("Data successfully fetched.\n" + data);
			}
		}
		
		if (GetCallback != null)
		{
			GetCallback (data);
		}
	}
	
	public delegate void _RemoveKey (bool success);
	/// <summary>
	/// The remove key callback.
	/// </summary>
	public _RemoveKey RemoveKeyCallback = null;
	
	/// <summary>
	/// Removes the specified key.
	/// </summary>
	/// <param name='key'>
	/// The key.
	/// </param>
	/// <param name='userData'>
	/// <c>true</c> if is user data; otherwise, <c>false</c>. Default false (i.e. game data).
	/// </param>
	public void RemoveKey (string key, bool userData = false)
	{
		if (key.Trim () == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Key is empty. Can't remove key.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Removing key.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("key", key);
				
		GJAPI.Instance.Request (DATA_REMOVE, parameters, userData, ReadRemoveKeyResponse);
	}
	
	/// <summary>
	/// Reads the remove key response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadRemoveKeyResponse (string response)
	{
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not remove key.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Key successfully removed.");
		}
		
		if (RemoveKeyCallback != null)
		{
			RemoveKeyCallback (success);
		}
	}
	
	public delegate void _GetKeysCallback (string[] keys);
	/// <summary>
	/// The get keys callback.
	/// </summary>
	public _GetKeysCallback GetKeysCallback = null;
	
	/// <summary>
	/// Gets the keys.
	/// </summary>
	/// <param name='userKeys'>
	/// <c>true</c> if is user keys; otherwise, <c>false</c>.  Default false (i.e. game data).
	/// </param>
	public void GetKeys (bool userKeys = false)
	{
		GJAPI.Instance.GJDebug ("Getting data keys.");
		
		GJAPI.Instance.Request (DATA_KEYS, null, userKeys, ReadGetKeysResponse);
	}
	
	/// <summary>
	/// Reads the get keys response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetKeysResponse (string response)
	{	
		GJAPI.Instance.GJDebug (response);
		
		string[] keys;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the data keys.\n" + response, LogType.Error);
			keys = null;
		}
		else
		{
			Dictionary<string, string> dictionary = GJAPI.Instance.ResponseToDictionary (response, true);
			GJAPI.Instance.CleanDictionary (ref dictionary, new string[] { "success0" });
				
			keys = new string [dictionary.Count];
			dictionary.Values.CopyTo (keys, 0);
			
			GJAPI.Instance.GJDebug ("Keys successfully fetched.\n" + string.Join ("\n", keys));
		}
		
		if (GetKeysCallback != null)
		{
			GetKeysCallback (keys);
		}
	}
}