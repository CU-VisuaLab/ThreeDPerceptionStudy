using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DxR;
using System.Linq;

public class SelectIndividualMark : MonoBehaviour {
    // Use this for initialization
    private GameObject menuObject;
    private List<GameObject> marks;

    private bool vrVersion;
    private bool clicked;
    private float width, height, depth;
    private bool confirming;

    private float minValue;
    private float maxValue;
    private string task; // Either "min" or "max"
    private GameObject taskDescription;

    private float startTime;
    private float menuTime;

    GameObject selectedObject;
    GameObject cameraObject;

    void Awake () {

        vrVersion = (GameObject.Find("GazeCursor") != null);

        if (Camera.main != null) cameraObject = Camera.main.gameObject;
        else cameraObject = GameObject.Find("Camera_eyes");

        transform.root.localScale = new Vector3(2, 2, 2);

        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.root.localScale.x * transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.root.localScale.y * transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.root.localScale.z * transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;

        if (taskDescription != null) taskDescription.transform.position = transform.position + new Vector3(width / 2, 1.3f * height, depth / 2);

        marks = new List<GameObject>();
        clicked = false;
        confirming = false;

        SetMinMaxValues();

        HandleTextFile.WriteString("Task Loaded at " + Time.time);
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if (vrVersion && clicked)
        {
            GameObject tempObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            clicked = false;
            if (tempObject != null)
            {
                if (confirming)
                {
                    if (tempObject.transform.name.Contains("Yes"))
                    {
                        YesButton();
                    }
                    else if (tempObject.transform.name.Contains("No"))
                    {
                        NoButton();
                    }
                }
                else
                {
                    //HandleTextFile.WriteString("Selected Value " + transform.GetComponent<Mark>().GetRealValue() + "; Menu Loaded at " + Time.time);
                    selectedObject = tempObject;
                    HandleTextFile.WriteString("> Time to Completion: " + (Time.time - startTime));
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
        }
        else if (!vrVersion && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.parent == transform)
                {
                    //HandleTextFile.WriteString("Selected Value " + hit.transform.GetComponent<Mark>().GetRealValue() + "; Menu Loaded at " + Time.time);
                    HandleTextFile.WriteString("> Time to Completion: " + (Time.time - startTime));
                    menuTime = Time.time;
                    selectedObject = hit.transform.gameObject;
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
            //menuObject.transform.localPosition = new Vector3(width / 2, height * 1.1f, depth / 2);
            menuObject.transform.LookAt(cameraObject.transform);
        }
        Vector3 center = transform.position + new Vector3(width / 2, height / 2, depth / 2);

        // If you're close, you want to render the menu next to the point
        if (Vector3.Distance(cameraObject.transform.position, center) < 1f)
        {
            float a = 0.05f;
            menuObject.transform.position = a * cameraObject.transform.position + (1 - a) * pos;
        }
        // If you're far, you want to render closer to the camera
        else
        {
            float a = 0.75f;
            menuObject.transform.position = a * cameraObject.transform.position + (1 - a) * center;
        }
        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Point?";

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);
    }

    private void SetMinMaxValues()
    {
        List<float> values = new List<float>();
        foreach (Transform child in transform)
        {
            values.Add(child.GetComponent<Mark>().GetRealValue());
        }
        minValue = values.Min();
        maxValue = values.Max();
    }

    private void YesButton()
    {
        HandleTextFile.WriteString("Selection confirmed at " + Time.time);
        HandleTextFile.WriteString("> Selected " + selectedObject.GetComponent<Mark>().GetRealValue());
        HandleTextFile.WriteString("> Min: " + minValue + " (delta=" + (selectedObject.GetComponent<Mark>().GetRealValue() - minValue) + "); Max: " +
            maxValue + " (delta=" + (selectedObject.GetComponent<Mark>().GetRealValue() - maxValue) + ")");
        if (!GameObject.Find("StudyInfrastructure").GetComponent<StudyInfrastructure>().LoadTrial())
        {
            // Load a message to tell the user they're done

            GameObject CompleteMessagePrefab = null;
            if (vrVersion)
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/VRStudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
                CompleteMessageObject.GetComponent<Canvas>().worldCamera = cameraObject.GetComponentInChildren<Camera>();
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
        HandleTextFile.WriteString("Selection rejected at " + Time.time);
        startTime = Time.time;
        // Reset the marks 
        foreach (GameObject mark in marks)
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

    public void setTask(string taskName)
    {
        GameObject taskDescriptionPrefab = Resources.Load("Prefabs/TaskDescription") as GameObject;
        taskDescription = Instantiate(taskDescriptionPrefab);
        taskDescription.transform.parent = transform.root;
        if (width > 0) taskDescription.transform.position = transform.position + new Vector3(width / 2, 1.3f * height, depth / 2);
        taskDescription.transform.localScale = taskDescription.transform.localScale * transform.root.localScale.x;
        if (taskName == "min")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "LOWEST";
        }
        else if (taskName == "max")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "HIGHEST";
        }

    }
}
