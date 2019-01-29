using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class MeasureAirTap : MonoBehaviour, IInputClickHandler
{

    [SerializeField] GameObject planes;
    [SerializeField] GameObject distanceLine;
    [SerializeField] MeasureText measureText;

    private float floorPosY; // 初期値:0

    void Start()
    {
        InputManager.Instance.AddGlobalListener(this.gameObject);

        planes.SetActive(false);
        distanceLine.SetActive(false);
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    void Update()
    {
        if (!(planes.activeSelf && distanceLine.activeSelf)) return;
        var heightMm = CalcHeightMm();
        distanceLine.transform.localScale = new Vector3(1, heightMm / 1000 / 2, 1);
        measureText.GetComponent<MeasureText>().height = heightMm;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        // 床面認識後にこの処理を走らせる
        if (!FloorPlaneController.isFloorPosYDone) return;

        var hitPos = GazeManager.Instance.HitPosition;
        this.gameObject.transform.position = hitPos;

        planes.SetActive(true);
        distanceLine.SetActive(true);
        this.gameObject.GetComponent<BoxCollider>().enabled = true;

        // 床面のY座標を取得する
        floorPosY = FloorPlaneController.FloorPosY;
    }

    float CalcHeightMm()
    {
        var tempHeight = (transform.position.y - floorPosY) * 1000; // 単位:mm
        var height = (float)Math.Round(tempHeight); // 四捨五入
        return height;
    }
}
