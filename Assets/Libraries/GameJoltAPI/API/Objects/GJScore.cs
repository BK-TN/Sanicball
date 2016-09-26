using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Game Jolt Score. Inherit from <see cref="GJObject"/>.
/// </summary>
public class GJScore : GJObject
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GJScore"/> class.
	/// </summary>
	public GJScore ()
	{	
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJScore"/> class.
	/// </summary>
	/// <param name='score'>
	/// The score string.
	/// </param>
	/// <param name='sort'>
	/// The score's numerical sort value.
	/// </param>
	public GJScore (string score, uint sort)
	{
		this.AddProperty ("score", score);
		this.AddProperty ("sort", sort.ToString ());
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJScore"/> class.
	/// </summary>
	/// <param name='properties'>
	/// Properties to add to the <see cref="GJScore"/>.
	/// </param>
	public GJScore (Dictionary<string,string> properties)
	{
		this.AddProperties (properties);
	}
	
	/// <summary>
	/// Gets or sets the score string.
	/// </summary>
	/// <value>
	/// The score string.
	/// </value>
	public string Score
	{
		get { return this.properties.ContainsKey ("score") ? this.properties ["score"] : ""; }
		set { this.properties ["score"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the score's numerical sort value.
	/// </summary>
	/// <value>
	/// The score's numerical sort value.
	/// </value>
	public uint Sort
	{
		get
		{
			if (this.properties.ContainsKey ("sort"))
			{
				if (this.properties ["sort"] == string.Empty)
				{
					Debug.Log ("Sort is empty. Returning 0.");
					return 0;
				}
				
				try
				{
					return Convert.ToUInt32 (this.properties ["sort"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting Score Sort to uint. Returning 0. " + e.Message);
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
		set { this.properties ["sort"] = value.ToString (); }
	}
	
	/// <summary>
	/// Gets or sets the extra data associated with the score.
	/// </summary>
	/// <value>
	/// The extra data associated with the score.
	/// </value>
	public string ExtraData
	{
		get { return this.properties.ContainsKey ("extra_data") ? this.properties ["extra_data"] : ""; }
		set { this.properties ["extra_data"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the name of the user (if this is a user score).
	/// </summary>
	/// <value>
	/// The name of the user (if this is a user score).
	/// </value>
	public string Username
	{
		get { return this.properties.ContainsKey ("user") ? this.properties ["user"] : ""; }
		set { this.properties ["user"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the identifier of the user (if this is a user score).
	/// </summary>
	/// <value>
	/// The identifier of the user (if this is a user score).
	/// </value>
	public uint UserID
	{
		get
		{
			if (this.properties.ContainsKey ("user_id"))
			{
				if (this.properties ["user_id"] == string.Empty)
				{
					Debug.Log ("User ID is empty. Returning 0.");
					return 0;
				}
				
				try
				{
					return Convert.ToUInt32 (this.properties ["user_id"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting User ID to uint. Returning 0. " + e.Message);
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
		set { this.properties ["user_id"] = value.ToString (); }
	}
	
	/// <summary>
	/// Gets or sets the name of the guest (if this is a guest score).
	/// </summary>
	/// <value>
	/// The name of the guest (if this is a guest score).
	/// </value>
	public string Guestname
	{
		get { return this.properties.ContainsKey ("guest") ? this.properties ["guest"] : ""; }
		set { this.properties ["guest"] = value; }
	}
	
	/// <summary>
	/// Gets the name of the user, wheter it's a user or a guest.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public string Name
	{
		get { return isUserScore ? Username : Guestname; }
	}
	
	/// <summary>
	/// Gets or sets when the score was logged by the user.
	/// </summary>
	/// <value>
	/// When the score was logged by the user.
	/// </value>
	public string Stored
	{
		get { return this.properties.ContainsKey ("stored") ? this.properties ["stored"] : ""; }
		set { this.properties ["stored"] = value; }
	}
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="GJScore"/> is user score.
	/// </summary>
	/// <value>
	/// <c>true</c> if is user score; otherwise, <c>false</c>.
	/// </value>
	public bool isUserScore
	{
		get { return (this.properties.ContainsKey ("user") && this.properties ["user"] != string.Empty); }
	}
}