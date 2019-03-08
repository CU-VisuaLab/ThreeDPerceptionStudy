﻿using DxR;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTrend : MonoBehaviour {
    public string selectionPlane = "XY";
    private float width, height, depth;

    private GameObject menuObject;

    private List<Vector3> directionalPoints;
    private List<GameObject> arrows;

    private bool vrVersion;

    private bool confirming;
    private bool clicked;

    // Use this for initialization
    void Start() {
        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;

        confirming = false;
        vrVersion = (GameObject.Find("GazeCursor") != null);

        SetDirections();
    }

    // Update is called once per frame
    void Update()
    {
        if (vrVersion && clicked && confirming)
        {
            GameObject hoveredObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            if (hoveredObject == null) return;

            if (hoveredObject.transform.name.Contains("Yes"))
            {
                YesButton();
            }
            else if (hoveredObject.transform.name.Contains("No"))
            {
                NoButton();
            }
            clicked = false;
        }
        else if ((!vrVersion && Input.GetMouseButtonDown(0)) || (vrVersion && clicked))
        {
            int selectedDirection = GetSelectedDirection();
            if (selectedDirection >= 0)
            {
                for (var i = 0; i < arrows.Count; i++)
                {
                    if (i != selectedDirection) arrows[i].SetActive(false);
                }
                InitializeConfirmationMenu(selectedDirection);
            }
            clicked = false;
        }
    }

    private void SetDirections()
    {
        Debug.Log(selectionPlane);
        if (arrows != null)
        {
            foreach (GameObject arrow in arrows)
            {
                Destroy(arrow);
            }
        }
        directionalPoints = new List<Vector3>();
        arrows = new List<GameObject>();
        Vector3 scaleVector = Vector3.zero;

        // Scale factor of 1 / 1000
        if (selectionPlane == "XY")
        {
            scaleVector = new Vector3(width / 2, height / 2, 0);
        }
        else if (selectionPlane == "XZ")
        {
            scaleVector = new Vector3(width / 2, 0, depth / 2);
        }
        else if (selectionPlane == "XYZ")
        {
            scaleVector = new Vector3(width / 2, height / 2, depth / 2);
        }

        if (selectionPlane == "XYZ")
        {
            // For XYZ Trend selection 
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.2f, 0, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.2f, 0, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 1.2f, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, -1.2f, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 0, 1.2f)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 0, -1.2f)));
        }
        else
        {
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.2f, 0, 0)));             // East
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.2f, 1.2f, 1.2f)));       // Northeast
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 1.2f, 1.2f)));          // North
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.2f, 1.2f, 1.2f)));      // Northwest 
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.2f, 0, 0)));            // West
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.2f, -1.2f, -1.2f)));    // Southwest
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, -1.2f, -1.2f)));        // South
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.2f, -1.2f, -1.2f)));     // Southeast
        }

        for (var i = 0; i < directionalPoints.Count; i++)
        {
            GameObject arrowPrefab = Resources.Load("Prefabs/Arrow") as GameObject;
            GameObject arrow = GameObject.Instantiate(arrowPrefab);
            arrow.transform.parent = transform;
            arrow.transform.localPosition = new Vector3(width / 2, height / 2, depth / 2) + directionalPoints[i];
            if (selectionPlane == "XY") arrow.transform.localEulerAngles = new Vector3(0, 0, -90 + 45 * i);
            else if (selectionPlane == "XZ") arrow.transform.localEulerAngles = new Vector3(0, 360 - 45 * i, -90);
            else if (selectionPlane == "XYZ")
            {
                if (i < 2) arrow.transform.localEulerAngles = arrow.transform.localEulerAngles = new Vector3(0, 0, -90 + 180 * i);
                else if (i < 4) arrow.transform.localEulerAngles = arrow.transform.localEulerAngles = new Vector3(180 * i, 0, 0);
                else arrow.transform.localEulerAngles = arrow.transform.localEulerAngles = new Vector3(90, 0, 180 * i);
            }
            arrows.Add(arrow);
        }
    }

    private void InitializeConfirmationMenu(int directionIndex)
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);

        menuObject.transform.parent = transform.root;
        menuObject.transform.position = arrows[directionIndex].transform.position + new Vector3(0, 0, -.05f);
        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Direction?";

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);

        confirming = true;
    }

    // Returns an index:
    // 0: E, 1: NE, 2: N, 3: NW, 4: W, 5: SW, 6: S, 7: SE
    private int GetSelectedDirection()
    {
        if (vrVersion)
        {
            GameObject hitObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            if (hitObject == null) return -1;

            for (var i = 0; i < arrows.Count; i++)
            {
                if (GameObject.ReferenceEquals(hitObject, arrows[i]))
                {
                    return i;
                }
            }
        }
        else
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                for (var i = 0; i < arrows.Count; i++)
                {
                    if (GameObject.ReferenceEquals(hit.transform.gameObject, arrows[i]))
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }

    private void YesButton()
    {
        if (!GameObject.Find("StudyInfrastructure").GetComponent<StudyInfrastructure>().LoadTrial())
        {
            // Load a message to tell the user they're done

            GameObject CompleteMessagePrefab = null;
            if (vrVersion)
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/VRStudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
                CompleteMessageObject.GetComponent<Canvas>().worldCamera = Camera.main;
            }
            else
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/StudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
            }
        }
        Destroy(transform.root.gameObject);
    }

    private void NoButton()
    {
        // Reset the quadrants 
        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(true);
        }
        confirming = false;
        Destroy(menuObject);
    }

    public void SetPlane(string plane)
    {
        selectionPlane = plane;
        SetDirections();
    }

    // VR Version: Click passed from Vive Controller
    public void Click()
    {
        clicked = true;
    }
}
