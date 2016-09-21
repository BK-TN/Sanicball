using UnityEngine;
using System.Collections;

/// <summary>
/// Game Jolt API Helper Scores Window. Inherit from <see cref="GJHWindow"/>
/// </summary>
public class GJHScoresWindow : GJHWindow
{
	/// <summary>
	/// The index and last of the tables toolbar.
	/// </summary>
	int tablesToolbarIndex = 0, lastTablesToolbarIndex = 0;
	/// <summary>
	/// The tables names.
	/// </summary>
	string[] tablesNames = null;
	/// <summary>
	/// The <see cref="GJTable"/>.
	/// </summary>
	GJTable[] tables = null;
	
	/// <summary>
	/// The scores scroll view position.
	/// </summary>
	Vector2 scoresScrollViewPosition;
	/// <summary>
	/// The scores.
	/// </summary>
	GJScore[] scores = null;
	
	/// <summary>
	/// The window states.
	/// </summary>
	enum ScoresWindowStates { ScoresList }
	
	/// <summary>
	/// GUI styles.
	/// </summary>
	GUIStyle userScoreStyle = null;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GJHScoresMethodsWindow"/> class.
	/// </summary>
	public GJHScoresWindow () : base ()
	{
		Title = "Leaderboards";
		float w = Screen.width * .9f;
		w = w > 400 ? 400 : w;
		float h = Screen.height * .9f;
		Position = new Rect (Screen.width / 2 - w / 2, Screen.height / 2 - h / 2, w, h);
		
		drawWindowDelegates.Add (ScoresWindowStates.ScoresList.ToString (), DrawScoresList);
		
		userScoreStyle = GJAPIHelper.Skin.FindStyle ("UserScore") ?? GJAPIHelper.Skin.label;
	}
	
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the <see cref="GJHScoresWindow"/> is
	/// reclaimed by garbage collection.
	/// </summary>
	~GJHScoresWindow ()
	{
		tables = null;
		scores = null;
		userScoreStyle = null;
	}
	
	/// <summary>
	/// Show this window.
	/// </summary>
	public override bool Show ()
	{
		bool success = base.Show ();
		if (success)
		{
			if (tablesNames != null) // Tables names has been cached.
			{
				GetScores ();
			}
			else
			{
				GetScoreTables ();
			}	
		}
		return success;
	}
	
	/// <summary>
	/// Dismiss this window.
	/// </summary>
	public override bool Dismiss ()
	{
		bool success = base.Dismiss ();
		if (success)
		{
			scores = null;
		}
		return success;
	}
	
	/// <summary>
	/// Gets the score tables. <seealso cref="GJTable"/>.
	/// </summary>
	void GetScoreTables ()
	{	
		SetWindowMessage ("Loading score tables");
		ChangeState (BaseWindowStates.Process.ToString ());
		
		tables = null;
		
		GJAPI.Scores.GetTablesCallback += OnGetScoreTables;
		GJAPI.Scores.GetTables ();
	}
	
	/// <summary>
	/// GetScoreTables callback.
	/// </summary>
	/// <param name='t'>
	/// The tables. <seealso cref="GJTable"/>.
	/// </param>
	void OnGetScoreTables (GJTable[] t)
	{
		GJAPI.Scores.GetTablesCallback -= OnGetScoreTables;
		
		if (t == null)
		{
			SetWindowMessage ("Error loading score tables.");
			ChangeState (BaseWindowStates.Error.ToString ());
			return;
		}
		
		tables = t;
		int count = t.Length;
		tablesNames = new string[count];
		for (int i = 0 ; i < count ; i++)
		{
			this.tablesNames[i] = t[i].Name;
		}
		
		GetScores ();
	}
	
	/// <summary>
	/// Gets the scores.
	/// </summary>
	void GetScores ()
	{	
		SetWindowMessage ("Loading Scores");
		ChangeState (BaseWindowStates.Process.ToString ());
		
		scores = null;
		
		GJAPI.Scores.GetMultipleCallback += OnGetScores;
		GJAPI.Scores.Get (false, tables[tablesToolbarIndex].Id, 100);
	}
	
	/// <summary>
	/// GetScores callback.
	/// </summary>
	/// <param name='s'>
	/// The scores. <seealso cref="GJScore"/>.
	/// </param>
	void OnGetScores (GJScore[] s)
	{
		GJAPI.Scores.GetMultipleCallback -= OnGetScores;
				
		if (s == null)
		{
			SetWindowMessage ("Error loading scores.");
			ChangeState (BaseWindowStates.Error.ToString ());
			return;
		}
		
		scores = s;

		ChangeState (ScoresWindowStates.ScoresList.ToString ());
	}
	
	/// <summary>
	/// Draws the scores list.
	/// </summary>
	void DrawScoresList ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		tablesToolbarIndex = GUILayout.Toolbar (tablesToolbarIndex, tablesNames);
		
		if (tablesToolbarIndex != lastTablesToolbarIndex)
		{
			lastTablesToolbarIndex = tablesToolbarIndex;
			GetScores ();
			return;
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		GUILayout.Space (10);
		
		scoresScrollViewPosition = GUILayout.BeginScrollView (scoresScrollViewPosition);
		int count = scores.Length;
		for (int i = 0 ; i < count ; i++)
		{
			DrawScore (i);
		}
		GUILayout.EndScrollView ();
		
		GUILayout.Space (10);
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Close"))
		{
			Dismiss (); 
		}
		GUILayout.EndHorizontal ();
	}
	
	/// <summary>
	/// Draws the score.
	/// </summary>
	/// <param name='s'>
	/// The score.
	/// </param>
	void DrawScore (int s)
	{
		if (
		GJAPI.User != null &&
		(GJAPI.User.Name == scores[s].Name ||
		(GJAPI.User.Type == GJUser.UserType.Developer && GJAPI.User.GetProperty ("developer_name") == scores[s].Name))
		)
		{
			GUILayout.BeginHorizontal (userScoreStyle);
		}
		else
		{
			GUILayout.BeginHorizontal ();
		}
		GUILayout.Label (scores[s].Name);
		GUILayout.FlexibleSpace ();
		GUILayout.Label (scores[s].Score);
		GUILayout.EndHorizontal ();
	}
}
