using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class GameSystemMessenger
{
    public static void WriteSaveData(string filename, string data)
    {
        PerformPCSave(filename, data);
        PerformPS3Save(filename, data);
    }
    private static void PerformPCSave(string filename, string data)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + filename))
        {
            sw.Write(data);
        }
        Debug.Log("Saved data to: " + fullPath);
#endif
    }
    private static void PerformPS3Save(string filename, string data)
    {
#if UNITY_PS3
        PS3SaveDataUtility.DoAutoSave(filename, data);
        while (!PS3SaveDataUtility.HasCompleted()) Debug.Log("Saving... DO NOT TURN OFF CONSOLE");
        switch (PS3SaveDataUtility.GetResult())
        {
            case PS3ReturnCode.Success:
                Debug.Log("Saved data to: " + filename);
                break;
        }
#endif
    }

    public static bool TryReadSaveData(string filename, out string dataString)
    {
        if(PerformPCLoad(filename, out dataString))
        {
            return true;
        }
        if(PerformPS3Load(filename, out dataString))
        {
            return true;
        }
        return false;
    }

    private static bool PerformPS3Load(string filename, out string dataString)
    {
        dataString = string.Empty;
        PS3SaveDataUtility.DoAutoLoad(filename); 
        while (!PS3SaveDataUtility.HasCompleted()) Debug.Log("Loading... DO NOT TURN OFF CONSOLE");
        switch (PS3SaveDataUtility.GetResult())
        {
            case PS3ReturnCode.Success:
                dataString = PS3SaveDataUtility.GetData();
                break;
        }
        return dataString.Length > 0;
    }

    private static bool PerformPCLoad(string filename, out string dataString)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        using (StreamReader sr = new StreamReader(fullPath))
        {
            dataString = sr.ReadToEnd();
        }
        return dataString.Length > 0;
    }
}