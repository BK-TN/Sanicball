using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Game Jolt High Score Table. Inherit from <see cref="GJObject"/>.
/// </summary>
public class GJTable : GJObject
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTable"/> class.
	/// </summary>
	public GJTable ()
	{	
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTable"/> class.
	/// </summary>
	/// <param name='id'>
	/// The table identifier.
	/// </param>
	/// <param name='name'>
	/// The table name.
	/// </param>
	/// <param name='primary'>
	/// Is the table a primary table.
	/// </param>
	/// <param name='description'>
	/// The table description. Default empty.
	/// </param>
	public GJTable (uint id, string name, bool primary, string description = "")
	{
		this.AddProperty ("id", id.ToString ());
		this.AddProperty ("name", name);
		this.AddProperty ("primary", primary.ToString ());
		this.AddProperty ("description", description);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJTable"/> class.
	/// </summary>
	/// <param name='properties'>
	/// Properties to add to the <see cref="GJTable"/>.
	/// </param>
	public GJTable (Dictionary<string,string> properties)
	{
		this.AddProperties (properties);
	}
	
	/// <summary>
	/// Gets or sets the high score table identifier.
	/// </summary>
	/// <value>
	/// The high score table identifier.
	/// </value>
	public uint Id
	{
		get
		{
			if (this.properties.ContainsKey ("id"))
			{
				if (this.properties ["id"] == string.Empty)
				{
					Debug.Log ("Table ID is empty. Returning 0.");
					return 0;
				}
				
				try
				{
					return Convert.ToUInt32 (this.properties ["id"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting Table ID to uint. Returning 0. " + e.Message);
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
	/// Gets or sets the high score table name.
	/// </summary>
	/// <value>
	/// The high score table name.
	/// </value>
	public string Name
	{
		get { return this.properties.ContainsKey ("name") ? this.properties ["name"] : string.Empty; }
		set { this.properties ["name"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the high score table description.
	/// </summary>
	/// <value>
	/// The high score table description.
	/// </value>
	public string Description
	{
		get { return this.properties.ContainsKey ("description") ? this.properties ["description"] : string.Empty; }
		set { this.properties ["description"] = value; }
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="GJTable"/> is a primary table. High scores are submitted to the primary table by default.
	/// </summary>
	/// <value>
	/// <c>true</c> if the table is a primary table; otherwise, <c>false</c>.
	/// </value>
	public bool Primary
	{
		get
		{ 
			if (this.properties.ContainsKey ("primary"))
			{
				return this.properties ["primary"] == "1";
			}
			else
			{
				return false;
			}
		}
		set { this.properties ["primary"] = value == true ? "1" : "0"; }
	}
}
