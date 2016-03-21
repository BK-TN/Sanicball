using UnityEngine;
using UnityEditor;
using System.Collections;

public class CountLines: EditorWindow 
{
	System.Text.StringBuilder strStats;
	Vector2 scrollPosition = new Vector2(0,0);
	struct File
	{
		public string 	name;
		public int 		nbLines;
		
		public File(string name, int nbLines)
		{
			this.name 		= name;
			this.nbLines 	= nbLines;
		}
	}	
	
	void OnGUI()
	{
		if (GUILayout.Button("Refresh"))
		{
			DoCountLines();
		}
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		EditorGUILayout.HelpBox(strStats.ToString(),MessageType.None);
		EditorGUILayout.EndScrollView();
	}
	
	
	[MenuItem("Custom/Stats/Count Lines")]
	public static void Init()
	{
		CountLines window = EditorWindow.GetWindow<CountLines>("Count Lines");
		window.Show();
		window.Focus();
		window.DoCountLines();
	}
	
	void DoCountLines()
	{		
		string strDir = System.IO.Directory.GetCurrentDirectory();
		strDir += @"/Assets";
		int iLengthOfRootPath = strDir.Length;
		ArrayList stats = new ArrayList();	
		ProcessDirectory(stats, strDir);	
		
		int iTotalNbLines = 0;
		foreach(File f in stats)
		{
			iTotalNbLines += f.nbLines;
		}
		
		strStats = new System.Text.StringBuilder();
		strStats.Append("Number of Files: " + stats.Count + "\n");		
		strStats.Append("Number of Lines: " + iTotalNbLines + "\n");	
		strStats.Append("================\n");	
		
		foreach(File f in stats)
		{
			strStats.Append(f.name.Substring(iLengthOfRootPath+1, f.name.Length-iLengthOfRootPath-1) + " --> " + f.nbLines + "\n");
		}		
	}
	
	static void ProcessDirectory(ArrayList stats, string dir)
	{	
		string[] strArrFiles = System.IO.Directory.GetFiles(dir, "*.cs");
		foreach (string strFileName in strArrFiles)
			ProcessFile(stats, strFileName);
		
		strArrFiles = System.IO.Directory.GetFiles(dir, "*.js");
		foreach (string strFileName in strArrFiles)
			ProcessFile(stats, strFileName);
		
		string[] strArrSubDir = System.IO.Directory.GetDirectories(dir);
		foreach (string strSubDir in strArrSubDir)
			ProcessDirectory(stats, strSubDir);
	}
	
	static void ProcessFile(ArrayList stats, string filename)
	{
		System.IO.StreamReader reader = System.IO.File.OpenText(filename);
		int iLineCount = 0;
		while (reader.Peek() >= 0)
		{
			reader.ReadLine();
			++iLineCount;
		}
		stats.Add(new File(filename, iLineCount));
		reader.Close();			
	}	
}