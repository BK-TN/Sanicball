using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Game Jolt API Scores mehods.
/// </summary>
public class GJScoresMethods
{
	const string
		SCORES_ADD = "scores/add/",
		SCORES_FETCH = "scores/",
		SCORES_TABLES = "scores/tables/";
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJScoresMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJScoresMethods ()
	{
		AddCallback = null;
		GetMultipleCallback = null;
		GetTablesCallback = null;
	}
	
	public delegate void _AddCallback (bool success);
	/// <summary>
	/// The add score callback.
	/// </summary>
	public _AddCallback AddCallback = null;
	
	/// <summary>
	/// Adds the specified score.
	/// </summary>
	/// <param name='score'>
	/// The score.
	/// </param>
	/// <param name='sort'>
	/// The score numeric value.
	/// </param>
	/// <param name='table'>
	/// The table. Default 0 (i.e. Main Table).
	/// </param>
	/// <param name='extraData'>
	/// Extra data. Default empty.
	/// </param>
	public void Add (string score, uint sort, uint table = 0, string extraData = "")
	{
		if (score.Trim () == string.Empty || sort == 0)
		{
			GJAPI.Instance.GJDebug ("Either score is empty or sort equal to zero (or both). Can't add score.", LogType.Error );
			return;
		}
		
		GJAPI.Instance.GJDebug ("Adding score for verified user: " + sort);
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("score", score);
		parameters.Add ("sort", sort.ToString ());
		if (extraData.Trim () != string.Empty)
		{
			parameters.Add ("extra_data", extraData);
		}
		if (table != 0)
		{
			parameters.Add ("table_id", table.ToString ());
		}
		
		GJAPI.Instance.Request (SCORES_ADD, parameters, true, ReadAddResponse);
	}
	
	/// <summary>
	/// Adds the specified guest score.
	/// </summary>
	/// <param name='score'>
	/// The score.
	/// </param>
	/// <param name='sort'>
	/// The score numerical value.
	/// </param>
	/// <param name='name'>
	/// The guest name. Default "Guest".
	/// </param>
	/// <param name='table'>
	/// The table. Default 0 (i.e. Main Table).
	/// </param>
	/// <param name='extraData'>
	/// Extra data. Default empty.
	/// </param>
	public void AddForGuest (string score, uint sort, string name = "Guest", uint table = 0, string extraData = "")
	{
		if (score.Trim () == string.Empty || sort == 0 || name == string.Empty)
		{
			GJAPI.Instance.GJDebug ("Either score is empty or sort equal to zero or name is empty (or all of them). Can't add score.", LogType.Error );
			return;
		}
		
		GJAPI.Instance.GJDebug ("Adding score for guest: " + sort);
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("score", score);
		parameters.Add ("sort", sort.ToString ());
		parameters.Add ("guest", name);
		if (extraData.Trim () != string.Empty)
		{
			parameters.Add ("extra_data", extraData);
		}
		if (table != 0)
		{
			parameters.Add ("table_id", table.ToString ());
		}
		
		GJAPI.Instance.Request (SCORES_ADD, parameters, false, ReadAddResponse);
	}
	
	/// <summary>
	/// Reads the add score response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadAddResponse (string response)
	{
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not add score.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Score successfully added.");
		}
		
		if (AddCallback != null)
		{
			AddCallback (success);
		}
	}
	
	public delegate void _GetMultipleCallback (GJScore[] scores);
	/// <summary>
	/// The get multiple score callback.
	/// </summary>
	public _GetMultipleCallback GetMultipleCallback = null;
	
	/// <summary>
	/// Get the specified scores.
	/// </summary>
	/// <param name='ofVerifiedUserOnly'>
	/// <c>true</c> to only fetch the scores of the verified user; otherwise, <c>false</c>. Default false (i.e. All).
	/// </param>
	/// <param name='table'>
	/// The table. Default 0 (i.e. Main Table).
	/// </param>
	/// <param name='limit'>
	/// The limit. Default 10.
	/// </param>
	public void Get (bool ofVerifiedUserOnly = false, uint table = 0, uint limit = 10)
	{
		if (limit == 0)
		{
			GJAPI.Instance.GJDebug ("Limit can't be equal to zero. Limit will be set to 1.", LogType.Warning);
			limit = 1;
		}
		else if (limit > 100)
		{
			GJAPI.Instance.GJDebug ("Limit can't be greater than 100. Limit will be set to 100.", LogType.Warning);
			limit = 100;
		}
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		parameters.Add ("limit", limit.ToString ());
		if (table != 0)
		{
			parameters.Add ("table_id", table.ToString ());
		}
		
		GJAPI.Instance.Request (SCORES_FETCH, parameters, ofVerifiedUserOnly, ReadGetResponse);
	}
	
	/// <summary>
	/// Reads the get multiple scores response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetResponse (string response)
	{
		GJScore[] scores;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not fetch scores.\n" + response, LogType.Error);
			scores = null;
		}
		else
		{
			Dictionary<string,string>[] dictionaries = GJAPI.Instance.ResponseToDictionaries (response);
			GJAPI.Instance.CleanDictionaries (ref dictionaries);
			
			StringBuilder debug = new StringBuilder();
			debug.Append ("Scores successfully fetched.\n");
			
			int count = dictionaries.Length;
			scores = new GJScore [count];
			for (int i = 0; i < count; i++)
			{
				scores [i] = new GJScore (dictionaries [i]);
				debug.Append (scores [i].ToString ());
			}
			
			GJAPI.Instance.GJDebug (debug.ToString ());
		}
		
		if (GetMultipleCallback != null)
		{
			GetMultipleCallback (scores);
		}
	}
	
	public delegate void _GetTablesCallback (GJTable[] tables);
	/// <summary>
	/// The get score tables callback.
	/// </summary>
	public _GetTablesCallback GetTablesCallback = null;
	
	/// <summary>
	/// Gets the score tables.
	/// </summary>
	public void GetTables ()
	{
		GJAPI.Instance.GJDebug ("Getting score tables.");
		
		GJAPI.Instance.Request (SCORES_TABLES, null, false, ReadGetTablesResponse);
	}
	
	/// <summary>
	/// Reads the get score tables response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadGetTablesResponse (string response)
	{
		GJTable[] tables;
		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not fetch score tables.\n" + response, LogType.Error);
			tables = null;
		}
		else
		{
			Dictionary<string,string>[] dictionaries = GJAPI.Instance.ResponseToDictionaries (response);
			GJAPI.Instance.CleanDictionaries (ref dictionaries);
			
			StringBuilder debug = new StringBuilder();
			debug.Append ("Score Tables successfully fetched.\n");
			
			int count = dictionaries.Length;
			tables = new GJTable [count];
			for (int i = 0; i < count; i++)
			{
				tables [i] = new GJTable (dictionaries [i]);
				debug.Append (tables [i].ToString ());
			}
			
			GJAPI.Instance.GJDebug (debug.ToString ());
		}
		
		if (GetTablesCallback != null)
		{
			GetTablesCallback (tables);
		}
	}
}