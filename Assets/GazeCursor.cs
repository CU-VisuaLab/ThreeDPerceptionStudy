﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GazeCursor : MonoBehaviour {

    public Color defaultColor;
    public Color highlightColor;
    public float defaultDistance = 1;
    private GameObject cameraObject;
    private GameObject hoveredObject;

    // Use this for initialization
    void Start() {
        InitializeCursor();
    }

    // Update is called once per frame
    void Update() {
        GetCursorPosition();
    }

    private void GetCursorPosition()
    {
        RaycastHit hit;
        Ray forwardRay = new Ray(cameraObject.transform.position, cameraObject.transform.forward);

        var layerMask = (1 << 8); // Ignore the "Cursor"--intersections with the cursor don't count
        layerMask = ~layerMask;

        if (Physics.Raycast(forwardRay, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            transform.Find("CursorObject").GetComponent<Renderer>().material.SetColor("_Color", highlightColor);
            hoveredObject = hit.transform.gameObject;
        }
        else
        {
            transform.localPosition = new Vector3(0, 0, defaultDistance);
            transform.localEulerAngles = Vector3.zero;
            transform.Find("CursorObject").GetComponent<Renderer>().material.SetColor("_Color", defaultColor);
            hoveredObject = null;
        }
    }

    public GameObject getHoveredObject()
    {
        return hoveredObject;
    }

    private void InitializeCursor()
    {
        if (Camera.main != null) cameraObject = Camera.main.gameObject;
        else cameraObject = GameObject.Find("Camera_eyes");
        transform.parent = cameraObject.transform;
        GameObject cursorObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cursorObject.transform.parent = transform;
        cursorObject.transform.name = "CursorObject";
        cursorObject.transform.localEulerAngles = new Vector3(90, 0, 0);
        cursorObject.transform.localScale = new Vector3(.02f, .005f, .02f);
        cursorObject.transform.localPosition = Vector3.zero;
        cursorObject.GetComponent<Collider>().enabled = false;
        cursorObject.GetComponent<Renderer>().material.SetColor("_Color", defaultColor);
        transform.localEulerAngles = Vector3.zero;
    }
}
