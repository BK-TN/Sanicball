using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Sessions mehods.
/// </summary>
public class GJSessionsMethods
{
	const string 	
		SESSIONS_OPEN = "sessions/open/",
		SESSIONS_PING = "sessions/ping/",
		SESSIONS_CLOSE = "sessions/close/";
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJSessionsMethods"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJSessionsMethods ()
	{
		OpenCallback = null;
		PingCallback = null;
		CloseCallback = null;
	}
	
	#region Open
	public delegate void _OpenCallback (bool success);
	/// <summary>
	/// The open session callback.
	/// </summary>
	public _OpenCallback OpenCallback = null;
	
	/// <summary>
	/// Open the session.
	/// </summary>
	public void Open ()
	{
		GJAPI.Instance.GJDebug ("Openning Session.");
		
		// Because we required authentification, there is no need to pass username and user_token to the Request method, it will be added automatically.
		// And because those are the only two parameters needed, we can pass null.
		GJAPI.Instance.Request (SESSIONS_OPEN, null, true, ReadOpenResponse);
	}
	
	/// <summary>
	/// Reads the open response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadOpenResponse (string response)
	{		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not open the session.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Session successfully opened.");
		}
		
		if (OpenCallback != null)
		{
			OpenCallback (success);
		}
	}
	#endregion Open
	
	#region Ping
	public delegate void _PingCallback (bool success);
	/// <summary>
	/// The ping session callback.
	/// </summary>
	public _PingCallback PingCallback = null;
	
	/// <summary>
	/// Ping the session.
	/// </summary>
	/// <param name='active'>
	/// <c>true</c> if active; otherwise, <c>false</c>. Default true.
	/// </param>
	public void Ping (bool active = true)
	{
		GJAPI.Instance.GJDebug ("Pinging Session.");
		
		Dictionary<string,string> parameters = new Dictionary<string, string> ();
		string status = active ? "active" : "idle";
		parameters.Add ("status", status);
		
		// Because we required authentification, there is no need to pass username and user_token to the Request method, it will be added automatically.
		GJAPI.Instance.Request (SESSIONS_PING, parameters, true, ReadPingResponse);
	}
	
	/// <summary>
	/// Reads the ping session response.
	/// </summary>
	/// <param name='response'>
	/// Response.
	/// </param>
	void ReadPingResponse (string response)
	{		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not ping the session.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Session successfully pinged.");
		}
		
		if (PingCallback != null)
		{
			PingCallback (success);
		}
	}
	#endregion Ping

	#region Close
	public delegate void _CloseCallback (bool success);
	/// <summary>
	/// The close session callback.
	/// </summary>
	public _CloseCallback CloseCallback = null;
	
	/// <summary>
	/// Close the session.
	/// </summary>
	public void Close ()
	{
		GJAPI.Instance.GJDebug ("Closing Session.");
		
		// Because we required authentification, there is no need to pass username and user_token to the Request method, it will be added automatically.
		// And because those are the only two parameters needed, we can pass null.
		GJAPI.Instance.Request (SESSIONS_CLOSE, null, true, ReadCloseResponse);
	}
	
	/// <summary>
	/// Reads the close session response.
	/// </summary>
	/// <param name='response'>
	/// The response.
	/// </param>
	void ReadCloseResponse (string response)
	{		
		bool success = GJAPI.Instance.IsResponseSuccessful (response);
		if (!success)
		{
			GJAPI.Instance.GJDebug ("Could not close the session.\n" + response, LogType.Error);
		}
		else
		{
			GJAPI.Instance.GJDebug ("Session successfully closed.");
		}
		
		if (CloseCallback != null)
		{
			CloseCallback (success);
		}
	}
	#endregion Close
}