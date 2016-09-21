using UnityEngine;
using System.Collections;

/// <summary>
/// The anchor of the <see cref="GJHNotification"/>.
/// </summary>
public enum GJHNotificationAnchor { TopLeft, TopCenter, TopRight, BottomLeft, BottomCenter, BottomRight }
/// <summary>
/// The type of the <see cref="GJHNotification"/>.
/// </summary>
public enum GJHNotificationType { Simple, WithIcon }

/// <summary>
/// Game Jolt API Helper Notification.
/// </summary>
public class GJHNotification
{
	/// <summary>
	/// The anchor position.
	/// </summary>
	GJHNotificationAnchor anchor = GJHNotificationAnchor.TopCenter;
	/// <summary>
	/// Gets or sets the anchor position.
	/// </summary>
	/// <value>
	/// The anchor position.
	/// </value>
	public GJHNotificationAnchor Anchor
	{
		get { return anchor; }
		set { anchor = value; SetPosition (); }
	}
	
	/// <summary>
	/// The type.
	/// </summary>
	GJHNotificationType type = GJHNotificationType.Simple;
	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	/// <value>
	/// The type.
	/// </value>
	public GJHNotificationType Type
	{
		get { return type; }
		set { type = value; SetPosition (); }
	}
	
	/// <summary>
	/// The display time.
	/// </summary>
	float displayTime = 5f;
	/// <summary>
	/// Gets or sets the display time.
	/// </summary>
	/// <value>
	/// The display time.
	/// </value>
	public float DisplayTime
	{
		get { return displayTime; }
		set { displayTime = value >= 1f ? value : 1f; } 
	}
	
	/// <summary>
	/// The position.
	/// </summary>
	Rect position;
	
	/// <summary>
	/// The title.
	/// </summary>
	public string Title = string.Empty;
	/// <summary>
	/// The description.
	/// </summary>
	public string Description = string.Empty;
	/// <summary>
	/// The icon.
	/// </summary>
	public Texture2D Icon = null;
	
	/// <summary>
	/// GUI styles.
	/// </summary>
	GUIStyle
		notificationBgStyle = null,
		notificationTitleStyle = null,
		notificationDescriptionStyle = null,
		smallNotificationTitleStyle = null;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHNotification"/> class.
	/// </summary>
	/// <param name='title'>
	/// The title.
	/// </param>
	/// <param name='description'>
	/// The description. Default empty.
	/// </param>
	/// <param name='icon'>
	/// The icon. Default null.
	/// </param>
	public GJHNotification (string title, string description = "", Texture2D icon = null)
	{
		Title = title;
		
		if (!string.IsNullOrEmpty (description))
		{
			Description = description;
			Icon = icon;
			type = GJHNotificationType.WithIcon;
		}
		else
		{
			type = GJHNotificationType.Simple;
		}
		
		SetPosition ();
		
		notificationBgStyle = GJAPIHelper.Skin.FindStyle ("NotificationBg") ?? GJAPIHelper.Skin.label;
		notificationTitleStyle = GJAPIHelper.Skin.FindStyle ("NotificationTitle") ?? GJAPIHelper.Skin.label;
		notificationDescriptionStyle = GJAPIHelper.Skin.FindStyle ("NotificationDescription") ?? GJAPIHelper.Skin.label;
		smallNotificationTitleStyle = GJAPIHelper.Skin.FindStyle ("SmallNotificationTitle") ?? GJAPIHelper.Skin.label;
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHNotification"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJHNotification ()
	{
		notificationBgStyle = null;
		notificationTitleStyle = null;
		notificationDescriptionStyle = null;
		smallNotificationTitleStyle = null;
		Icon = null;
	}
	
	/// <summary>
	/// Draw the notification.
	/// </summary>
	public void OnGUI ()
	{
		switch (type)
		{
		default:
		case GJHNotificationType.Simple:
			DrawSmallNotification ();
			break;
		case GJHNotificationType.WithIcon:
			DrawMediumNotification ();
			break;
		}
	}
	
	/// <summary>
	/// Draws the small notification.
	/// </summary>
	void DrawSmallNotification ()
	{
		GUI.BeginGroup (position, notificationBgStyle);
		GUI.Label (new Rect (0, 0, position.width, position.height), Title, smallNotificationTitleStyle);
		GUI.EndGroup ();
	}
	
	/// <summary>
	/// Draws the medium notification.
	/// </summary>
	void DrawMediumNotification ()
	{
		GUI.BeginGroup (position, notificationBgStyle);
		GUI.DrawTexture (new Rect (10, 10, 75, 75), Icon);
		GUI.Label (new Rect (100, 10, 290, 20), Title, notificationTitleStyle);
		GUI.Label (new Rect (100, 40, 290, 45), Description, notificationDescriptionStyle);
		GUI.EndGroup ();
	}
	
	/// <summary>
	/// Sets the position.
	/// </summary>
	void SetPosition ()
	{
		switch (Type)
		{
		default:
		case GJHNotificationType.Simple:
			position = new Rect (0, 0, 250, 25);
			break;
		case GJHNotificationType.WithIcon:
			position = new Rect (0, 0, 400, 95);
			break;
		}
		
		switch (Anchor)
		{
		default:
		case GJHNotificationAnchor.TopLeft:
			position.x = 10f;
			position.y = 10f;
			break;
		case GJHNotificationAnchor.TopCenter:
			position.x = Screen.width / 2  - position.width / 2;
			position.y = 10f;
			break;
		case GJHNotificationAnchor.TopRight:
			position.x = Screen.width - 10f - position.width;
			position.y = 10f;
			break;
		case GJHNotificationAnchor.BottomLeft:
			position.x = 10f;
			position.y = Screen.height - 10f - position.height;
			break;
		case GJHNotificationAnchor.BottomCenter:
			position.x = Screen.width / 2  - position.width / 2;
			position.y = Screen.height - 10f - position.height;
			break;
		case GJHNotificationAnchor.BottomRight:
			position.x = Screen.width - 10f - position.width;
			position.y = Screen.height - 10f - position.height;
			break;
		}
	}
}