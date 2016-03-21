using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Game Jolt Object. Base class for all Game Jolt Object.
/// </summary>
public abstract class GJObject
{	
	/// <summary>
	/// A dictionary containing all the properties of the Game Jolt Object.
	/// </summary>
	protected Dictionary<string, string> properties;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJObject"/> class.
	/// </summary>
	public GJObject ()
	{
		properties = new Dictionary<string, string>();
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJObject"/> is reclaimed
	/// by garbage collection.
	/// </summary>
	~GJObject ()
	{
		properties = null;
	}
	
	/// <summary>
	/// Adds the property to the Game Jolt Object.
	/// </summary>
	/// <param name='key'>
	/// The Key of the property to add.
	/// </param>
	/// <param name='value'>
	/// The Value of the property to add.
	/// </param>
	/// <param name='overwrite'>
	/// Overwrite the property if it already exist. Default false.
	/// </param>
	public void AddProperty (string key, string value, bool overwrite = false)
	{
		if (properties.ContainsKey (key) && !overwrite)
		{
			return;
		}
		
		properties [key] = value;
	}
	
	/// <summary>
	/// Adds the properties to the Game Jolt Object.
	/// </summary>
	/// <param name='properties'>
	/// The properties to add to the Game Jolt Object.
	/// </param>
	/// <param name='overwrite'>
	/// Overwrite the properties that already exist. Default false.
	/// </param>
	public void AddProperties (Dictionary<string,string> properties, bool overwrite = false)
	{
		foreach (KeyValuePair<string,string> pair in properties)
		{
			AddProperty (pair.Key, pair.Value, overwrite);
		}
	}
	
	/// <summary>
	/// Gets the property of the Game Jolt Object.
	/// </summary>
	/// <returns>
	/// The property or an empty string if the property couldn't be found.
	/// </returns>
	/// <param name='key'>
	/// The Key of the property to get.
	/// </param>
	public string GetProperty (string key)
	{
		return this.properties.ContainsKey (key) ? properties [key] : string.Empty;
	}
	
	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="GJObject"/>.
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/> that represents the current <see cref="GJObject"/>.
	/// </returns>
	public override string ToString ()
	{
		StringBuilder output = new StringBuilder();
		output.AppendFormat (" [{0}]\n", this.GetType().ToString());
		foreach (KeyValuePair<string,string> pair in properties)
		{
			output.AppendFormat ("{0}: {1}\n", pair.Key, pair.Value);
		}
		
		return output.ToString ();
	}
}