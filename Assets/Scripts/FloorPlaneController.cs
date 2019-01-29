using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;

public class FloorPlaneController : MonoBehaviour, IInputClickHandler
{
    public static float FloorPosY { get; private set; }
    public static bool isFloorPosYDone { get; private set; }

    [SerializeField] GameObject FloorPlane;
    [SerializeField] UnityEvent SpatialMappingDisable = new UnityEvent();

    void Start()
    {
        InputManager.Instance.AddGlobalListener(this.gameObject);

        FloorPlane.SetActive(false);
    }

    void Update()
    {
        var hitObj = GazeManager.Instance.HitObject;
        if (hitObj == null || hitObj.layer != 31)
        {
            FloorPlane.SetActive(false);
            return;
        }

        var hitPos = GazeManager.Instance.HitPosition;
        if (Camera.current.transform.position.y < hitPos.y)
        {
            FloorPlane.SetActive(false);
            return;
        }

        //hitPos.y += 0.001f; // FloorPlaneを1mm浮かす
        FloorPlane.transform.position = hitPos;

        var angles = Quaternion.LookRotation(Camera.current.transform.position - FloorPlane.transform.position).eulerAngles;
        FloorPlane.transform.rotation = Quaternion.Euler(0f, angles.y, angles.z);
        FloorPlane.SetActive(true);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        var hitObj = GazeManager.Instance.HitObject;
        if (hitObj == null || hitObj.layer != 31) return;

        var y = GazeManager.Instance.HitPosition.y;
        FloorPosY = y;

        FloorPlane.SetActive(false);
        isFloorPosYDone = true;
        SpatialMappingDisable.Invoke();
    }
}
