using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class GameSystemMessenger
{
    public static void WriteSaveData(string filename, string data)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + filename))
        {
            sw.Write(data);
        }
        Debug.Log("Saved data to: " + fullPath);
#endif
#if UNITY_PS3
        PS3SaveDataUtility.DoAutoSave(filename, data);
#endif
    }

    public static bool TryReadSaveData(string filename, out string dataString)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string fullPath = Path.Combine(Application.persistentDataPath,filename);
        using (StreamReader sr = new StreamReader(fullPath))
        {
            dataString = sr.ReadToEnd();
        }
        return dataString.Length > 0;
#endif
    }
}