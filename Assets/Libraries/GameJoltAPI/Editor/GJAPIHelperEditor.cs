using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (GJAPIHelper))]
public class GJAPIHelperEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space (10);
		EditorGUILayout.HelpBox ("I'm here to help.", MessageType.Info, true);
		GUILayout.Space (10);
	}
}
