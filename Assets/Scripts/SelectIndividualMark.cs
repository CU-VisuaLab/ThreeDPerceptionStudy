﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectIndividualMark : MonoBehaviour {
    // Use this for initialization
    private GameObject menuObject;
    private List<GameObject> marks;

    private bool vrVersion;
    private bool clicked;

    void Start () {
        marks = new List<GameObject>();
        vrVersion = (GameObject.Find("GazeCursor") != null);
        clicked = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (vrVersion && clicked)
        {
            GameObject selectedObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            Debug.Log(selectedObject);
            if (selectedObject != null)
            {
                foreach (Transform child in transform)
                {
                    if (child != selectedObject)
                    {
                        marks.Add(child.gameObject);
                        child.gameObject.SetActive(false);
                    }
                }
                InitializeConfirmationMenu(selectedObject.transform.position);
            }
            clicked = false;
        }
        else if (!vrVersion && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.parent == transform)
                {
                    foreach (Transform child in transform)
                    {
                        if (child != hit.transform)
                        {
                            marks.Add(child.gameObject);
                            child.gameObject.SetActive(false);
                        }
                    }
                    InitializeConfirmationMenu(hit.transform.position);
                }
            }
        }
    }

    private void InitializeConfirmationMenu(Vector3 pos)
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);

        menuObject.transform.parent = transform;
        menuObject.transform.position = pos + new Vector3(0, 0, -.05f);

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);
    }

    private void YesButton()
    {
        if (!GameObject.Find("StudyInfrastructure").GetComponent<StudyInfrastructure>().LoadTrial())
        {
            // Load a message to tell the user they're done
            GameObject CompleteMessagePrefab = Resources.Load("Prefabs/StudyCompleteMessage") as GameObject;
            GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
        }
        Destroy(transform.root.gameObject);
    }

    private void NoButton()
    {
        // Reset the marks 
        foreach(GameObject mark in marks)
        {
            mark.SetActive(true);
        }
        Destroy(menuObject);
    }

    // VR Version: Click passed from Vive Controller
    public void Click()
    {
        clicked = true;
    }
}
