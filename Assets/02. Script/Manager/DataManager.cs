using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    /// <summary>
    /// index -> Score, money etc.. : Data -> "Score","Money"..
    /// </summary>
    /// <param name="index"></param>
    /// <param name="Data"></param>
    public static void SaveData(int index, string Data)
    {
        if(PlayerPrefs.GetInt(Data) < index)
        PlayerPrefs.SetInt(Data, index);
    }

    public static int LoadData(string Data)
    {
        return PlayerPrefs.GetInt(Data);
    }
}
