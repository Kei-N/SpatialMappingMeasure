using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeasureText : MonoBehaviour
{

    public float height;

    void Update()
    {
        if (height == 0) return;
        GetComponent<Text>().text = height.ToString() + "m";
    }
}
