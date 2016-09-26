using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Jolt API Helper Notifications Manager. Inherit from <see cref="MonoBehaviour"/>
/// </summary>
public class GJHNotificationsManager : MonoBehaviour
{
	#region Singleton Pattern
	/// <summary>
	/// The <see cref="GJHNotificationsManager"/> instance.
	/// </summary>
	private static GJHNotificationsManager instance;
	/// <summary>
	/// Gets the <see cref="GJHNotificationsManager"/> instance.
	/// </summary>
	/// <value>
	/// The <see cref="GJHNotificationsManager"/> instance.
	/// </value>
	public static GJHNotificationsManager Instance
	{
		get
		{
			if (instance == null)
			{
				GJAPIHelper gjapih = (GJAPIHelper) FindObjectOfType (typeof (GJAPIHelper));
				
				if (gjapih == null)
				{
					Debug.LogError ("An instance of GJAPIHelper is needed in the scene, but there is none. Can't initialise GJHNotificationsManager.");
				}
				else
				{
					instance = gjapih.gameObject.AddComponent<GJHNotificationsManager>();
					
					if (instance == null)
					{
						Debug.Log ("An error occured creating GJHNotificationsManager.");
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
		queue = null;
		currentNotification = null;
		instance = null;
	}
	#endregion Singleton Pattern
	
	/// <summary>
	/// The notifications queue.
	/// </summary>
	Queue<GJHNotification> queue = new Queue<GJHNotification>();
	/// <summary>
	/// The current notification.
	/// </summary>
	GJHNotification currentNotification = null;
	
	/// <summary>
	/// The current notification appear time.
	/// </summary>
	float currentNotificationAppearTime = 0f;
	
	/// <summary>
	/// Queues the notification.
	/// </summary>
	/// <param name='notification'>
	/// The notification.
	/// </param>
	public static void QueueNotification (GJHNotification notification)
	{
		Instance.queue.Enqueue (notification);
	}
	
	/// <summary>
	/// Draw the nofications.
	/// </summary>
	void OnGUI ()
	{	
		if (currentNotification != null)
		{
			if (Time.time > currentNotificationAppearTime + currentNotification.DisplayTime)
			{
				currentNotification = null;
			}
			else
			{
				if (GJAPIHelper.Skin != null)
					GUI.skin = GJAPIHelper.Skin;
				
				currentNotification.OnGUI ();
			}
		}
		else
		{
			if (queue.Count > 0)
			{
				currentNotification = queue.Dequeue ();
				currentNotificationAppearTime = Time.time;
			}
		}
	}
}
