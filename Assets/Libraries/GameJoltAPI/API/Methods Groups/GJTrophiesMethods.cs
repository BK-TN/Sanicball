using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Game Jolt API Trophies mehods.
/// </summary>
public class GJTrophiesMethods
{
	const string
		TROPHIES_ADD = "trophies/add-achieved/",
		TROPHIES_FETCH = "trophies/";
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJTrophiesMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJTrophiesMethods ()
	{
		AddCallback = null;
		GetOneCallback = null;
		GetMultipleCallback = null;
		GetAllCallback = null;
	}
	
	public delegate void _AddTrophyCallback (bool success);
	/// <summary>
	/// The add trophy callback.
	/// </summary>
	public _AddTrophyCallback AddCallback = null;
	
	/// <summary>
	/// Add the specified trophy.
	/// </summary>
	/// <param name='id'>
	/// The trophy identifier.
	/// </param>
	public void Add (uint id)
	{
		if (id == 0)
		{
			GJAPI.Instance.GJDebug ("ID is null. Can't add Trophy.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Adding Trophy: " + id + ".");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add ("trophy_id", id.ToString ());
		
		GJAPI.Instance.Request (TROPHIES_ADD, parameters, true, ReadAddResponse);
	}
	
	/// <summary>
	/// Reads the add trophy response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadAddResponse (string response)
	{
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not add Trophy.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Trophy successfully added.");
		}
		
		if (AddCallback != null)
		{
			AddCallback (success);
		}
	}
	
	public delegate void _GetOneCallback (GJTrophy trophy);
	/// <summary>
	/// The get one trophy callback.
	/// </summary>
	public _GetOneCallback GetOneCallback = null;
	
	/// <summary>
	/// Get the specified trophy.
	/// </summary>
	/// <param name='id'>
	/// The trophy identifier.
	/// </param>
	public void Get (uint id)
	{
		if (id == 0)
		{
			GJAPI.Instance.GJDebug ("ID is null. Can't get trophy.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting Trophy: " + id + ".");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add ("trophy_id", id.ToString ());
		
		GJAPI.Instance.Request (TROPHIES_FETCH, parameters, true, ReadGetOneResponse);
	}
	
	/// <summary>
	/// Reads the get one trophy response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetOneResponse (string response)
	{
		GJTrophy trophy;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the trophy.\n" + response, LogType.Error);
			trophy = null;
		}
		else
		{			
			Dictionary<string,string> dictionary = GJAPI.Instance.ResponseToDictionary (response);
			GJAPI.Instance.CleanDictionary (ref dictionary);
			
			trophy = new GJTrophy (dictionary);
			GJAPI.Instance.GJDebug ("Trophy successfully fetched.\n" + trophy.ToString());
		}
		
		if (GetOneCallback != null)
		{
			GetOneCallback (trophy);
		}
	}
	
	public delegate void _GetMultipleCallback (GJTrophy[] trophies);
	/// <summary>
	/// The get multiple trophies callback.
	/// </summary>
	public _GetMultipleCallback GetMultipleCallback = null;
	
	/// <summary>
	/// Get the specified trophies.
	/// </summary>
	/// <param name='ids'>
	/// The trophies identifiers.
	/// </param>
	public void Get (uint[] ids)
	{
		if (ids == null)
		{
			GJAPI.Instance.GJDebug ("IDs are null. Can't get trophies.", LogType.Error);
			return;
		}
		
		GJAPI.Instance.GJDebug ("Getting trophies.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		string joinedIds = string.Join (",", new List<uint>(ids).ConvertAll (i => i.ToString ()).ToArray ());
		parameters.Add ("trophy_id", joinedIds);
		
		GJAPI.Instance.Request (TROPHIES_FETCH, parameters, true, ReadGetMultipleResponse);
	}
	
	/// <summary>
	/// Reads the get multiple trophies response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetMultipleResponse (string response)
	{
		GJTrophy[] trophies;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the trophies.\n" + response, LogType.Error);
			trophies = null;
		}
		else
		{			
			Dictionary<string,string>[] dictionaries = GJAPI.Instance.ResponseToDictionaries (response);
			GJAPI.Instance.CleanDictionaries (ref dictionaries);
			
			StringBuilder debug = new StringBuilder();
			debug.Append ("Trophies successfully fetched.\n");
			
			int count = dictionaries.Length;
			trophies = new GJTrophy [count];
			for (int i = 0; i < count; i++)
			{
				trophies [i] = new GJTrophy (dictionaries [i]);
				debug.Append (trophies [i].ToString ());
			}
			
			GJAPI.Instance.GJDebug (debug.ToString ());
		}
		
		if (GetMultipleCallback != null)
		{
			GetMultipleCallback (trophies);
		}
	}
	
	public delegate void _GetAllCallback (GJTrophy[] trophies);
	/// <summary>
	/// The get all trophies callback.
	/// </summary>
	public _GetAllCallback GetAllCallback = null;
	
	/// <summary>
	/// Gets all trophies.
	/// </summary>
	public void GetAll ()
	{
		GJAPI.Instance.GJDebug ("Getting all trophies.");
		
		GJAPI.Instance.Request (TROPHIES_FETCH, null, true, ReadGetAllResponse);
	}
	
	/// <summary>
	/// Gets all trophies.
	/// </summary>
	/// <param name='achieved'>
	/// <c>true</c> to fetch only achieved trophies; <c>false</c> to fetch only unachieved trophies.
	/// </param>
	public void GetAll (bool achieved)
	{
		if (achieved)
		{
			GJAPI.Instance.GJDebug ("Getting all trophies the verified user has achieved.");
		}
		else
		{
			GJAPI.Instance.GJDebug ("Getting all trophies the verified user hasn't achieved.");
		}
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("achieved", achieved.ToString ().ToLower ());
		
		GJAPI.Instance.Request (TROPHIES_FETCH, parameters, true, ReadGetAllResponse);
	}
	
	/// <summary>
	/// Reads the get all trophies response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetAllResponse (string response)
	{
		GJTrophy[] trophies;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not get the trophies.\n" + response, LogType.Error);
			trophies = null;
		}
		else
		{			
			Dictionary<string,string>[] dictionaries = GJAPI.Instance.ResponseToDictionaries (response);
			GJAPI.Instance.CleanDictionaries (ref dictionaries);
			
			StringBuilder debug = new StringBuilder();
			debug.Append ("Trophies successfully fetched.\n");
			
			int count = dictionaries.Length;
			trophies = new GJTrophy [count];
			for (int i = 0; i < count; i++)
			{
				trophies [i] = new GJTrophy (dictionaries [i]);
				debug.Append (trophies [i].ToString ());
			}
			
			GJAPI.Instance.GJDebug (debug.ToString ());
		}
		
		if (GetAllCallback != null)
		{
			GetAllCallback (trophies);
		}
	}
}