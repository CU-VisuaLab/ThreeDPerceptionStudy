using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DxR;

public class SelectIndividualMark : MonoBehaviour {
    // Use this for initialization
    private GameObject menuObject;
    private List<GameObject> marks;

    private bool vrVersion;
    private bool clicked;
    private float width, height, depth;
    private bool confirming;

    void Start () {

        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;

        marks = new List<GameObject>();
        vrVersion = (GameObject.Find("GazeCursor") != null);
        clicked = false;
        confirming = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (vrVersion && clicked)
        {
            GameObject selectedObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            if (selectedObject != null) Debug.Log(selectedObject.transform.name);
            if (selectedObject != null)
            {
                if (confirming)
                {

                    if (selectedObject.transform.name.Contains("Yes"))
                    {
                        YesButton();
                    }
                    else if (selectedObject.transform.name.Contains("No"))
                    {
                        NoButton();
                    }
                }
                else
                {
                    foreach (Transform child in transform)
                    {
                        if (child.gameObject != selectedObject)
                        {
                            marks.Add(child.gameObject);
                            child.gameObject.SetActive(false);
                        }
                    }
                    InitializeConfirmationMenu(selectedObject.transform.position);
                }
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

        confirming = true;
        menuObject.transform.parent = transform.root;
        if (vrVersion)
        {
            menuObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            /*Vector3 middlePos = transform.root.transform.position + new Vector3(width / 2, height / 2, depth / 2);
            float distance = Vector3.Distance(Camera.main.transform.position, middlePos);
            float ratio = 1.02f * Vector3.Magnitude(new Vector3(width / 2, height / 2, depth / 2)) / distance;


            menuObject.transform.position = ratio * Camera.main.transform.position + new Vector3(0, 0, -.05f) +
                (1f - ratio) * (transform.root.position + new Vector3(width / 2, height / 2, depth / 2));*/
            menuObject.transform.localPosition = new Vector3(width / 2, height * 1.1f, depth / 2);
            menuObject.transform.LookAt(Camera.main.transform);
        }
        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Point?";

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);
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
        // Reset the marks 
        foreach(GameObject mark in marks)
        {
            mark.SetActive(true);
        }
        Destroy(menuObject);
        confirming = false;
    }

    // VR Version: Click passed from Vive Controller
    public void Click()
    {
        clicked = true;
    }
}
