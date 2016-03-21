using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (GJHNotificationsManager))]
public class GJHNotificationsManagerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space (10);
		EditorGUILayout.HelpBox ("I manage the Game Jolt API notifications.", MessageType.Info, true);
		GUILayout.Space (10);
	}
}
