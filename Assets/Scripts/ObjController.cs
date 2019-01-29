using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class ObjController : MonoBehaviour {

    [SerializeField] GameObject[] objFiles;

    private void Start()
    {
        MoveObject();
    }

    private void MoveObject()
    {
        foreach(GameObject objFile in objFiles)
        {
            var jsonStr = JsonImporter.JsonToString(objFile.name);
            if (jsonStr.Equals("error")) return;
            var json = JsonConvert.DeserializeObject<PointC>(jsonStr);

            objFile.transform.position = new Vector3(json.PosX, json.PosY, json.PosZ); // objファイルを移動させる
            objFile.transform.Rotate(new Vector3(0, json.RotY, 0)); // objファイルを回転させる

            Debug.Log("PosX : " + json.PosX);
            Debug.Log("PosY : " + json.PosY);
            Debug.Log("PosZ : " + json.PosZ);
            Debug.Log("RotY : " + json.RotY);
        }
        Debug.Log("MoveObject() : Done");

    }
    private class PointC
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotY { get; set; }
    }

}
