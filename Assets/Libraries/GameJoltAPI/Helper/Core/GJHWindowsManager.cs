using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Helper Windows Manager. Inherit from <see cref="MonoBehaviour"/>
/// </summary>
public class GJHWindowsManager : MonoBehaviour
{
	#region Singleton Pattern
	/// <summary>
	/// The <see cref="GJHWindowsManager"/> instance.
	/// </summary>
	private static GJHWindowsManager instance;
	/// <summary>
	/// Gets the <see cref="GJHWindowsManager"/> instance.
	/// </summary>
	/// <value>
	/// The <see cref="GJHWindowsManager"/> instance.
	/// </value>
	public static GJHWindowsManager Instance
	{
		get
		{
			if (instance == null)
			{
				GJAPIHelper gjapih = (GJAPIHelper) FindObjectOfType (typeof (GJAPIHelper));
				
				if (gjapih == null)
				{
					Debug.LogError ("An instance of GJAPIHelper is needed in the scene, but there is none. Can't initialise GJHWindowManager.");
				}
				else
				{
					instance = gjapih.gameObject.AddComponent<GJHWindowsManager>();
					
					if (instance == null)
					{
						Debug.Log ("An error occured creating GJHWindowManager.");
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
		windows = null;
		instance = null;
	}
	#endregion Singleton Pattern

	/// <summary>
	/// The windows.
	/// </summary>
	List <GJHWindow> windows = null;
	/// <summary>
	/// The current window.
	/// </summary>
	int currentWindow = -1;
	
	/// <summary>
	/// Init the GJHWindowsManager.
	/// </summary>
	void Awake ()
	{
		windows = new List<GJHWindow> ();
	}
	
	/// <summary>
	/// Registers the window.
	/// </summary>
	/// <returns>
	/// The window identifier.
	/// </returns>
	/// <param name='window'>
	/// The window to register.
	/// </param>
	public static int RegisterWindow (GJHWindow window)
	{
		Instance.windows.Add (window);
		return Instance.windows.Count - 1;
	}
	
	/// <summary>
	/// Shows the window.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the specified window was shown; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='index'>
	/// The window identifer.
	/// </param>
	public static bool ShowWindow (int index)
	{
		if (index < 0)
		{
			Debug.Log ("GJAPIHelper: The index of the window can't be negative. Can't show the window " + index);
			return false;
		}
		else if (index >= Instance.windows.Count)
		{
			Debug.Log ("GJAPIHelper: The index of the window is out of range. Can't show the window " + index);
			return false;
		}
		
		if (Instance.currentWindow != -1)
		{
			if (Instance.currentWindow == index)
			{
				Debug.Log (
					"GJAPIHelper: The window \""
					+ Instance.windows[index].Title + "\" is already showing.");
				return false;
			}
			else
			{
				Debug.Log (
					"GJAPIHelper: " + Instance.windows[Instance.currentWindow].Title + " window is already showing. Can't show \""
					+ Instance.windows[index].Title + "\" window.");
				return false;
			}
		}
		else 
		{
			Instance.currentWindow = index;
			return true;
		}
	}
	
	/// <summary>
	/// Dismisses the window.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the specified window was dismissed; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='index'>
	/// If set to <c>true</c> index.
	/// </param>
	public static bool DismissWindow (int index)
	{
		if (index < 0)
		{
			Debug.Log ("GJAPIHelper: The index of the window can't be negative. Can't dismiss the window " + index);
			return false;
		}
		else if (index >= Instance.windows.Count)
		{
			Debug.Log ("GJAPIHelper: The index of the window is out of range. Can't dismiss the window " + index);
			return false;
		}
		
		if (Instance.currentWindow != index)
		{
			Debug.Log (
					"GJAPIHelper: The window \""
					+ Instance.windows[index].Title + "\" isn't already showing. Can't dismiss it.");
			return false;
		}
		else
		{
			Instance.currentWindow = -1;
			return true;
		}
	}
	
	/// <summary>
	/// Determines whether this window is showing.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance this window is showing; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='index'>
	/// The window identifier.
	/// </param>
	public static bool IsWindowShowing (int index)
	{
		return Instance.currentWindow == index;
	}
	
	void OnGUI ()
	{
		if (currentWindow != -1)
		{
			windows[currentWindow].OnGUI ();
		}
	}
}