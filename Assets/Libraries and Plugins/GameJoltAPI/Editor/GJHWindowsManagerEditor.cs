using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (GJHWindowsManager))]
public class GJHWindowsManagerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space (10);
		EditorGUILayout.HelpBox ("I manage the Game Jolt API windows.", MessageType.Info, true);
		GUILayout.Space (10);
	}
}
