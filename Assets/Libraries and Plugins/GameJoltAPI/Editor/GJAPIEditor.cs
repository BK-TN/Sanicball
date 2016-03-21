using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (GJAPI))]
public class GJAPIEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space (10);
		
		if (GJAPI.GameID == 0)
		{
			EditorGUILayout.HelpBox ("Initialize the API to see its information.", MessageType.Warning, true);
		}	
		else
		{
			GUILayout.Label ("API Information", EditorStyles.toolbarButton, GUILayout.ExpandWidth (true));
			
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Game ID", GUILayout.Width (75));
			EditorGUILayout.SelectableLabel (GJAPI.GameID.ToString (), GUILayout.Height (14));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Private Key", GUILayout.Width (75));
			EditorGUILayout.SelectableLabel (GJAPI.PrivateKey, EditorStyles.wordWrappedLabel, GUILayout.Height (28));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Verbose", GUILayout.Width (75));
			GJAPI.Verbose = GUILayout.Toggle (GJAPI.Verbose, "");
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Version", GUILayout.Width (75));
			EditorGUILayout.SelectableLabel (GJAPI.Version.ToString (), GUILayout.Height (14));
			GUILayout.EndHorizontal ();
			
			GUILayout.Space (10);
			GUILayout.Label ("User Information", EditorStyles.toolbarButton, GUILayout.ExpandWidth (true));
			
			if (GJAPI.User == null)
			{
				EditorGUILayout.HelpBox ("Verify a user to see its information.", MessageType.Info, true);
			}
			else
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Name", GUILayout.Width (75));
				EditorGUILayout.SelectableLabel (GJAPI.User.Name, GUILayout.Height (14));
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Token", GUILayout.Width (75));
				EditorGUILayout.SelectableLabel (GJAPI.User.Token, GUILayout.Height (14));
				GUILayout.EndHorizontal ();
				
				if (GJAPI.User.Id == 0)
				{
					GUILayout.Space (5);
					EditorGUILayout.HelpBox ("Fetch the verified user to see more information.", MessageType.Info, true);
				}
				else
				{
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("ID", GUILayout.Width (75));
					EditorGUILayout.SelectableLabel (GJAPI.User.Id.ToString (), GUILayout.Height (14));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Type", GUILayout.Width (75));
					EditorGUILayout.SelectableLabel (GJAPI.User.Type.ToString (), GUILayout.Height (14));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Status", GUILayout.Width (75));
					EditorGUILayout.SelectableLabel (GJAPI.User.Status.ToString (), GUILayout.Height (14));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Avatar URL", GUILayout.Width (75));
					EditorGUILayout.SelectableLabel (GJAPI.User.AvatarURL, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight (true));
					GUILayout.EndHorizontal ();
				}
			}
		}
		
		GUILayout.Space (10);
	}
}
