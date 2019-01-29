using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;


public class JsonImporter : MonoBehaviour
{
    public static string JsonToString(string jsonFileName)
    {
        string pointCInfo;
        // FileReadTest.txtファイルを読み込む
        FileInfo fi = new FileInfo("Assets\\VerificationData\\Json\\" + jsonFileName + ".json");
        try
        {
            // 一行毎読み込み
            using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
            {
                pointCInfo = sr.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            Debug.Log("JsonToString() : error");
            return "error";
        }

        return pointCInfo;
    }
}
