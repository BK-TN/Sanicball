using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Game Jolt User. Inherit from <see cref="GJObject"/>.
/// </summary>
public class GJUser : GJObject
{
	/// <summary>
	/// User type enumeration.
	/// </summary>
	public enum UserType { Undefined, User, Developer, Moderator, Admin };
	
	/// <summary>
	/// User status enumeration.
	/// </summary>
	public enum UserStatus { Undefined, Active, Banned };
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJUser"/> class.
	/// </summary>
	public GJUser ()
	{	
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJUser"/> class.
	/// </summary>
	/// <param name='username'>
	/// The user username.
	/// </param>
	/// <param name='user_token'>
	/// The user token.
	/// </param>
	public GJUser (string username, string user_token)
	{
		this.AddProperty ("username", username);
		this.AddProperty ("user_token", user_token);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJUser"/> class.
	/// </summary>
	/// <param name='properties'>
	/// Properties to add to the <see cref="GJUser"/>.
	/// </param>
	public GJUser (Dictionary<string,string> properties)
	{
		this.AddProperties (properties);
	}
	
	/// <summary>
	/// Gets or sets the user name.
	/// </summary>
	/// <value>
	/// The user name.
	/// </value>
	public string Name
	{
		get { return this.properties.ContainsKey ("username") ? this.properties ["username"] : string.Empty; }
		set { this.properties ["username"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the user token.
	/// </summary>
	/// <value>
	/// The user token.
	/// </value>
	public string Token
	{
		get { return this.properties.ContainsKey ("user_token") ? this.properties ["user_token"] : string.Empty; }
		set { this.properties ["user_token"] = value; }
	}
	
	/// <summary>
	/// Gets or sets the user identifier.
	/// </summary>
	/// <value>
	/// The user identifier.
	/// </value>
	public uint Id
	{
		get
		{
			if (this.properties.ContainsKey ("id"))
			{
				if (this.properties ["id"] == string.Empty)
				{
					Debug.Log ("User ID is empty. Returning 0.");
					return 0;
				}
				
				try
				{
					return Convert.ToUInt32 (this.properties ["id"]);
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
		set { this.properties ["id"] = value.ToString (); }
	}
	
	/// <summary>
	/// Gets or sets the user type. See <see cref="GJTrophy.UserType"/>.
	/// </summary>
	/// <value>
	/// The user type. See <see cref="GJTrophy.UserType"/>.
	/// </value>
	public UserType Type
	{
		get
		{
			if (this.properties.ContainsKey ("type"))
			{
				try 
				{
					return (UserType) Enum.Parse (typeof (UserType), this.properties ["type"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting User Type to UserType. Value will be Undefined. " + e.Message);
					return UserType.Undefined;
				}
			}
			else
			{
				return UserType.Undefined;
			}
		}
		set { this.properties ["type"] = value.ToString(); }
	}
	
	/// <summary>
	/// Gets or sets the user status. See <see cref="GJTrophy.UserStatus"/>.
	/// </summary>
	/// <value>
	/// The status. See <see cref="GJTrophy.UserStatus"/>.
	/// </value>
	public UserStatus Status
	{
		get
		{
			if (this.properties.ContainsKey ("status"))
			{
				try 
				{
					return (UserStatus) Enum.Parse (typeof (UserStatus), this.properties ["status"]);
				}
				catch (Exception e)
				{
					Debug.LogError ("Error converting User Status to UserStatus. Value will be Undefined. " + e.Message);
					return UserStatus.Undefined;
				}
			}
			else
			{
				return UserStatus.Undefined;
			}
		}
		set { this.properties ["status"] = value.ToString(); }
	}
	
	/// <summary>
	/// Gets the URL of the user's avatar.
	/// </summary>
	/// <value>
	/// The URL of the user's avatar.
	/// </value>
	public string AvatarURL
	{
		get { return this.properties.ContainsKey ("avatar_url") ? this.properties ["avatar_url"] : string.Empty; }
		set { this.properties ["avatar_url"] = value; }
	}
}