using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Helper Window.
/// </summary>
public class GJHWindow
{
	#region Properties
	/// <summary>
	/// The title.
	/// </summary>
	public string Title = string.Empty;
	/// <summary>
	/// The position.
	/// </summary>
	public Rect Position;
	
	/// <summary>
	/// The identifier.
	/// </summary>
	int windowID = 0;
	
	/// <summary>
	/// The window states.
	/// </summary>
	protected enum BaseWindowStates { Empty, Process, Success, Error };
	/// <summary>
	/// The previous state of the window.
	/// </summary>
	string previousWindowState = string.Empty;
	/// <summary>
	/// The current state of the window.
	/// </summary>
	string currentWindowState = BaseWindowStates.Empty.ToString();
	
	/// <summary>
	/// Draw delegate.
	/// </summary>
	protected delegate void DrawWindowDelegate ();
	/// <summary>
	/// The draw window delegates.
	/// </summary>
	protected Dictionary < string, DrawWindowDelegate > drawWindowDelegates = null;
	
	/// <summary>
	/// The window message.
	/// </summary>
	string windowMsg = string.Empty;
	/// <summary>
	/// The state the window should return to when done.
	/// </summary>
	string windowReturnState = string.Empty;
	
	/// <summary>
	/// GUI styles.
	/// </summary>
	GUIStyle
		errorStyle = null,
		successStyle = null,
		ellipsisStyle = null;
	#endregion Properties
	
	#region Initialisation
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHWindow"/> class.
	/// </summary>
	public GJHWindow ()
	{
		Title = "Base Window";
		Position = new Rect (Screen.width / 2 - 150, Screen.height / 2 - 50, 300, 100);
		windowID = GJHWindowsManager.RegisterWindow (this);
		
		drawWindowDelegates = new Dictionary < string, DrawWindowDelegate > ();
		drawWindowDelegates.Add (BaseWindowStates.Empty.ToString (), DrawWindowEmpty);
		drawWindowDelegates.Add (BaseWindowStates.Process.ToString (), DrawWindowProcessing);
		drawWindowDelegates.Add (BaseWindowStates.Success.ToString (), DrawWindowSuccess);
		drawWindowDelegates.Add (BaseWindowStates.Error.ToString (), DrawWindowError);
		
		errorStyle = GJAPIHelper.Skin.FindStyle ("ErrorMsg") ?? GJAPIHelper.Skin.label;
		successStyle = GJAPIHelper.Skin.FindStyle ("SuccessMsg") ?? GJAPIHelper.Skin.label;
		ellipsisStyle = GJAPIHelper.Skin.FindStyle ("Ellipsis") ?? GJAPIHelper.Skin.label;
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHWindow"/> is reclaimed
	/// by garbage collection.
	/// </summary>
	~GJHWindow ()
	{
		drawWindowDelegates = null;
		errorStyle = null;
		successStyle = null;
		ellipsisStyle = null;
	}
	#endregion Initialisation
	
	#region Public Interface
	/// <summary>
	/// Determines whether this window is showing.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this window is showing; otherwise, <c>false</c>.
	/// </returns>
	public bool IsShowing ()
	{
		return GJHWindowsManager.IsWindowShowing (windowID);
	}
	
	/// <summary>
	/// Show this window.
	/// </summary>
	public virtual bool Show ()
	{
		return GJHWindowsManager.ShowWindow (windowID);
	}
	
	/// <summary>
	/// Dismiss this window.
	/// </summary>
	public virtual bool Dismiss ()
	{
		return GJHWindowsManager.DismissWindow (windowID);
	}
	#endregion Public Interface
	
	#region GUI
	/// <summary>
	/// Draws the window.
	/// </summary>
	public void OnGUI ()
	{
		if (GJAPIHelper.Skin != null)
			GUI.skin = GJAPIHelper.Skin;
		
		#if UNITY_3_5
		GUI.Window (windowID, Position, DrawWindow, Title);
		#else
		GUI.ModalWindow (windowID, Position, DrawWindow, Title);	
		#endif
	}

	/// <summary>
	/// Draws the window.
	/// </summary>
	/// <param name='windowID'>
	/// The window identifier.
	/// </param>
	void DrawWindow (int windowID)
	{
		if (drawWindowDelegates.ContainsKey (currentWindowState))
		{
			BeginWindow ();
			drawWindowDelegates[currentWindowState] (); 
			EndWindow ();
		}
		else
		{
			Debug.Log ("Unknown window state. Can't draw the window.");
		}
	}
	
	/// <summary>
	/// Draws the empty window.
	/// </summary>
	void DrawWindowEmpty ()
	{
		GUILayout.Label ("I'm an empty window. Nobody likes me ;-(");
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Close"))
		{
			Dismiss ();
		}
	}
	
	/// <summary>
	/// Draws the processing window.
	/// </summary>
	void DrawWindowProcessing ()
	{
		if (!string.IsNullOrEmpty (windowMsg))
		{
			GUILayout.Label (windowMsg);
		}
		GUILayout.Label (AnimatedEllipsis (5, 1.5f), ellipsisStyle);
		GUILayout.FlexibleSpace ();
	}
	
	/// <summary>
	/// Draws the success window.
	/// </summary>
	void DrawWindowSuccess ()
	{
		if (!string.IsNullOrEmpty (windowMsg))
		{
			GUILayout.Label (windowMsg, successStyle);
		}
		GUILayout.FlexibleSpace ();
		if (windowReturnState != string.Empty)
		{
			if (GUILayout.Button ("Ok"))
			{
				ChangeState (windowReturnState);
			}
		}
		else
		{
			if (GUILayout.Button ("Close"))
			{
				Dismiss ();
			}			
		}
	}
	
	/// <summary>
	/// Draws the error window.
	/// </summary>
	void DrawWindowError ()
	{
		if (!string.IsNullOrEmpty (windowMsg))
		{
			GUILayout.Label (windowMsg, errorStyle);
		}
		GUILayout.FlexibleSpace ();
		if (windowReturnState != string.Empty)
		{
			if (GUILayout.Button ("Ok"))
			{
				ChangeState (windowReturnState);
			}
		}
		else
		{
			if (GUILayout.Button ("Close"))
			{
				Dismiss ();
			}			
		}
	}
	#endregion GUI
	
	#region GUI Helpers
	/// <summary>
	/// Changes the window state.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the state was successfully changed; <c>false</c> otherwise.
	/// </returns>
	/// <param name='state'>
	/// The new state.
	/// </param>
	protected bool ChangeState (string state)
	{
		if (!drawWindowDelegates.ContainsKey (state))
		{
			Debug.LogWarning ("No such state exist. Can't change window state.");
			return false;
		}
		previousWindowState = currentWindowState;
		currentWindowState = state;
		return true;
	}
	
	/// <summary>
	/// Reverts the window to its previous state.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the state was successfully reverted; <c>false</c> otherwise.
	/// </returns>
	protected bool RevertToPreviousState ()
	{
		if (string.IsNullOrEmpty (previousWindowState.Trim ()))
		{
			Debug.LogWarning ("No previous state found. Can't revert to previous window state.");
			return false;
		}
		
		string cur = currentWindowState;
		currentWindowState = previousWindowState;
		previousWindowState = cur;
		return true;
	}
	
	/// <summary>
	/// Sets the window message.
	/// </summary>
	/// <param name='msg'>
	/// The message.
	/// </param>
	/// <param name='returnState'>
	/// The return state.
	/// </param>
	protected void SetWindowMessage (string msg, string returnState = "")
	{		
		windowMsg = msg;
		windowReturnState = returnState;
	}
	
	/// <summary>
	/// Begins the window.
	/// </summary>
	protected void BeginWindow ()
	{
		GUILayout.Space (35);
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
	}
	
	/// <summary>
	/// Ends the window.
	/// </summary>
	protected void EndWindow ()
	{
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
	}
	
	/// <summary>
	/// Animate the ellipsis.
	/// </summary>
	/// <returns>
	/// An animated ellipsis.
	/// </returns>
	/// <param name='amount'>
	/// The amount of dot.
	/// </param>
	/// <param name='speed'>
	/// The speed of the animation.
	/// </param>
	protected string AnimatedEllipsis (int amount = 3, float speed = 1f)
	{
		return new string ('.', (int) (Time.time * speed) % (amount + 1));
	}
	#endregion GUI Helpers
}