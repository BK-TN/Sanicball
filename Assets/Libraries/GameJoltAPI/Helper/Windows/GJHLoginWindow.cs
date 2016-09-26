using UnityEngine;
using System.Collections;

/// <summary>
/// Game Jolt API Helper Login Window. Inherit from <see cref="GJHWindow"/>
/// </summary>
public class GJHLoginWindow : GJHWindow
{	
	/// <summary>
	/// The username.
	/// </summary>
	string uName = string.Empty;
	/// <summary>
	/// The user token.
	/// </summary>
	string uToken = string.Empty;
	
	/// <summary>
	/// The window states.
	/// </summary>
	enum LoginWindowStates { LoginForm }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHLoginWindow"/> class.
	/// </summary>
	public GJHLoginWindow () : base ()
	{
		Title = "Login to Game Jolt";
		Position = new Rect (Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200);
		
		drawWindowDelegates.Add (LoginWindowStates.LoginForm.ToString (), DrawForm);
	}
	
	/// <summary>
	/// Show this window.
	/// </summary>
	public override bool Show ()
	{
		bool success = base.Show ();
		if (success)
		{
			ChangeState (LoginWindowStates.LoginForm.ToString ());
		}
		return success;
	}
	
	/// <summary>
	/// Dismiss this window.
	/// </summary>
	public override bool Dismiss ()
	{
		return base.Dismiss ();
	}	
	
	/// <summary>
	/// Draws the login form.
	/// </summary>
	void DrawForm ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Username", GUILayout.Width (100));
		uName = GUILayout.TextField (uName, GUILayout.Width (150));
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Token", GUILayout.Width (100));
		uToken = GUILayout.PasswordField (uToken, '*', GUILayout.Width (150));
		GUILayout.EndHorizontal ();
		
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Login"))
		{
			if (uName.Trim () == string.Empty || uToken.Trim () == string.Empty)
			{
				SetWindowMessage ("Either Username or Token is empty.", LoginWindowStates.LoginForm.ToString ());
				ChangeState (BaseWindowStates.Error.ToString ());
			}
			else
			{
				GJAPI.Users.VerifyCallback += OnVerifyUser;
				GJAPI.Users.Verify (uName, uToken);
				
				SetWindowMessage ("Connecting");
				ChangeState (BaseWindowStates.Process.ToString ());
			}
		}
		if (GUILayout.Button ("Cancel"))
		{
			Dismiss ();
		}
	}
	
	/// <summary>
	/// OnVerifyUser callback.
	/// </summary>
	/// <param name='success'>
	/// <c>true</c> if the user was successfully verified; <c>false</c> otherwise.
	/// </param>
	void OnVerifyUser (bool success)
	{
		GJAPI.Users.VerifyCallback -= OnVerifyUser;
		
		if (!success)
		{
			SetWindowMessage ("Error logging in.\nPlease check your username and token.", LoginWindowStates.LoginForm.ToString ());
			ChangeState (BaseWindowStates.Error.ToString ());
		}
		else
		{
			SetWindowMessage (string.Format ("Hello {0}!", uName));
			ChangeState (BaseWindowStates.Success.ToString ());
		}
	}
}
