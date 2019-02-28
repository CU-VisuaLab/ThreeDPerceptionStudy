using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GazeCursor : MonoBehaviour {

    public Color defaultColor;
    public Color highlightColor;
    public float defaultDistance = 3;
    private GameObject cameraObject;
    private GameObject hoveredObject;

    // Use this for initialization
    void Start() {
        cameraObject = Camera.main.gameObject;
        transform.parent = cameraObject.transform;
        transform.Find("CursorObject").GetComponent<Renderer>().material.SetColor("_Color", defaultColor);
        Debug.Log(cameraObject.transform.name);
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

        if (Physics.Raycast(forwardRay, out hit, defaultDistance, layerMask))
        {
            transform.position = hit.point;
            transform.Find("CursorObject").GetComponent<Renderer>().material.SetColor("_Color", highlightColor);
            Vector3 incomingVec = hit.point - cameraObject.transform.position;
            Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

            transform.eulerAngles = reflectVec;

            hoveredObject = hit.transform.gameObject;
        }
        else
        {
            transform.localPosition = new Vector3(0, 0, defaultDistance) / transform.lossyScale.z;
            transform.Find("CursorObject").GetComponent<Renderer>().material.SetColor("_Color", defaultColor);
            hoveredObject = null;
        }
    }

    public GameObject getHoveredObject()
    {
        return hoveredObject;
    }
}
