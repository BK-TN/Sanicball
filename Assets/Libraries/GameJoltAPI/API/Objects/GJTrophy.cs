using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Game Jolt Trophy.  Inherit from <see cref="GJObject"/>.
/// </summary>
public class GJTrophy : GJObject
{
	/// <summary>
	/// Trophy difficulty enumeration.
	/// </summary>
	public enum TrophyDifficulty { Undefined, Bronze, Silver, Gold, Platinium };
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTrophy"/> class.
	/// </summary>
	public GJTrophy ()
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTrophy"/> class.
	/// </summary>
	/// <param name='id'>
	/// The trophy identifier.
	/// </param>
	/// <param name='title'>
	/// The trophy name.
	/// </param>
	/// <param name='difficulty'>
	/// The trophy difficulty. See <see cref="GJTrophy.TrophyDifficulty"/>.
	/// </param>
	/// <param name='achieved'>
	///  Is the trophy achieved.
	/// </param>
	/// <param name='description'>
	/// The trophy description. Default empty.
	/// </param>
	/// <param name='imageURL'>
	/// The URL to the trophy's thumbnail. Default empty.
	/// </param>
	public GJTrophy (uint id, string title, TrophyDifficulty difficulty, bool achieved, string description = "", string imageURL = "")
	{
		this.AddProperty ("id", id.ToString ());
		this.AddProperty ("title", title);
		this.AddProperty ("difficulty", difficulty.ToString ());
		this.AddProperty ("achieved", achieved.ToString ());
		this.AddProperty ("description", description);
		this.AddProperty ("image_url", imageURL);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTrophy"/> class.
	/// </summary>
	/// <param name='properties'>
	/// Properties to add to the <see cref="GJTrophy"/>.
	/// </param>
	public GJTrophy (Dictionary<string,string> properties)
	{
		this.AddProperties (properties);
	}
	
	/// <summary>
	/// Gets or sets the trophy identifier.
	/// </summary>
	/// <value>
	/// The trophy identifier.
	/// </value>
	public uint Id
	{
		get
		{
			if (this.properties.ContainsKey ("id"))
			{
				if (this.properties ["id"] == string.Empty)
				{
					Debug.Log ("Trophy ID is empty. Returning 0.");
					return 0;
				}
				
				try
				{
					return Convert.ToUInt32 (this.properties ["id"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting Trophy ID to uint. Returning 0. " + e.Message);
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
		set { this.properties ["id"] = value.ToString (); }
	}
		
	/// <summary>
	/// Gets or sets the trophy name.
	/// </summary>
	/// <value>
	/// The trophy name.
	/// </value>
	public string Title
	{
		get { return this.properties.ContainsKey ("title") ? this.properties ["title"] : string.Empty; }
		set { this.properties ["title"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	/// <value>
	/// The description.
	/// </value>
	public string Description
	{
		get { return this.properties.ContainsKey ("description") ? this.properties ["description"] : string.Empty; }
		set { this.properties ["description"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the trophy difficulty. See <see cref="GJTrophy.TrophyDifficulty"/>.
	/// </summary>
	/// <value>
	/// The trophy difficulty. See <see cref="GJTrophy.TrophyDifficulty"/>.
	/// </value>
	public TrophyDifficulty Difficulty
	{
		get
		{
			if (this.properties.ContainsKey ("difficulty"))
			{
				try
				{
					return (TrophyDifficulty) Enum.Parse (typeof (TrophyDifficulty), this.properties ["difficulty"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting Trophy Difficulty to TrophyDifficulty. Returning Undefined. " + e.Message);
					return TrophyDifficulty.Undefined;
				}
			}
			else
			{
				return TrophyDifficulty.Undefined;
			}
		}
		set { this.properties ["difficulty"] = value.ToString ();}
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="GJTrophy"/> is achieved.
	/// </summary>
	/// <value>
	/// <c>true</c> if achieved; otherwise, <c>false</c>.
	/// </value>
	public bool Achieved
	{
		get
		{
			if (this.properties.ContainsKey ("achieved") && !(this.properties ["achieved"] == "false"))
			{
				return true;
			}
			return false;
		}
		set { this.properties ["achieved"] = value.ToString (); }
	}
	
	/// <summary>
	/// Gets or sets when the trophy was achieved.
	/// </summary>
	/// <value>
	/// When the trophy was achieved.
	/// </value>
	public string AchievedTime
	{
		get
		{
			if (this.properties.ContainsKey ("achieved") && !(this.properties ["achieved"] == "false"))
			{
				return this.properties ["achieved"];
			}
			return "NA";
		}
		set { this.properties ["achieved"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the URL of the trophy's thumbnail.
	/// </summary>
	/// <value>
	/// The URL of the trophy's thumbnail.
	/// </value>
	public string ImageURL
	{
		get { return this.properties.ContainsKey ("image_url") ? this.properties ["image_url"] : string.Empty; }
		set { this.properties ["image_url"] = value.ToString (); }
	}	
}