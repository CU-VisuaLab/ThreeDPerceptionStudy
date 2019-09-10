using DxR;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectQuadrant : MonoBehaviour {
    public string selectionPlane = "XY";
    private float width, height, depth;
    
    private GameObject menuObject;
    private List<GameObject> marks;
    private List<Vector3> quadrantCenters;
    private List<GameObject> quadrants;
    private Vector3 selectedQuadrant;
    
    private bool confirming;
    private bool clicked;
    private bool vrVersion;
    
    private string task; // Either "min" or "max"
    private GameObject taskDescription;

    private Dictionary<Vector3,float> quadrantAverages;

    private float startTime;
    private float menuTime;

    private GameObject cameraObject;

    private float incorrectTime;
    private bool training;

    // Use this for initialization
    void Awake ()
    {
        vrVersion = (GameObject.Find("GazeCursor") != null);

        if (Camera.main != null) cameraObject = Camera.main.gameObject;
        else cameraObject = GameObject.Find("Camera_eyes");

        vrVersion = (GameObject.Find("GazeCursor") != null);

        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.root.localScale.x * transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.root.localScale.y * transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.root.localScale.z * transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;

        if (taskDescription != null) taskDescription.transform.position = transform.position + new Vector3(width / 2, 1.3f * height, depth / 2);

        confirming = false;

        clicked = false;

        training = FindObjectOfType<CSVReader>().csvFile.name.ToLower().Contains("train");
        HandleTextFile.WriteString("Task Loaded at " + Time.time);
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (vrVersion && clicked)
        {
            clicked = false;
            if (confirming)
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
            }
            else
            {
                selectedQuadrant = GetSelectedQuadrant();
                if (selectedQuadrant.x == Mathf.NegativeInfinity) return;
                HandleTextFile.WriteString("Selected Quadrant " + selectedQuadrant + " with Average " + quadrantAverages[selectedQuadrant] + "; Menu Loaded at " + Time.time);
                FindObjectOfType<StudyInfrastructure>().HandleClick();
                HandleTextFile.WriteString("> Time to Completion: " + (Time.time - startTime));
                
            }
        }
        else if (!vrVersion && Input.GetMouseButtonDown(0) && !confirming)
        {
            selectedQuadrant = GetSelectedQuadrant();
            if (selectedQuadrant.x == Mathf.NegativeInfinity) return;
            HandleTextFile.WriteString("Selected Quadrant " + selectedQuadrant + " with Average " + quadrantAverages[selectedQuadrant] + "; Menu Loaded at " + Time.time);
            FindObjectOfType<StudyInfrastructure>().HandleClick();
            HandleTextFile.WriteString("> Time to Completion: " + (Time.time - startTime));
        }

        if (training && GameObject.Find("TaskHUD").GetComponent<Text>().text.Contains("Incorrect") && Time.time > incorrectTime)
        {
            GameObject.Find("TaskHUD").GetComponent<Text>().text = "";
        }
    }

    private void SetQuadrants()
    {
        quadrantCenters = new List<Vector3>();
        quadrants = new List<GameObject>();
        int offsetVector = 1;
        if (transform.root.localEulerAngles.y == 180)
        {
            offsetVector = -1;
        }
        if (selectionPlane == "XY")
        {
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, height / 4, depth / 2));         // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, height / 4, depth / 2));     // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, 3 * height / 4, depth / 2));     // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, 3 * height / 4, depth / 2)); // Top Right
            
        }
        else if (selectionPlane == "XZ")
        {
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, height / 2, depth / 4));          // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, height / 2, depth / 4));      // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, height / 2, 3 * depth / 4));      // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, height / 2, 3 * depth / 4));  // Top Right
        }
        else if (selectionPlane == "XYZ")
        {
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, height / 4, depth / 4));              // Front Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, height / 4, depth / 4));          // Front Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, 3 * height / 4, depth / 4));          // Front Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, 3 * height / 4, depth / 4));      // Front Top Right
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, height / 4, 3 * depth / 4));          // Back Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, height / 4, 3 * depth / 4));      // Back Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(offsetVector * width / 4, 3 * height / 4, 3 * depth / 4));      // Back Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * offsetVector * width / 4, 3 * height / 4, 3 * depth / 4));  // Back Top Right    
        }
    }
    private Vector3 GetGazedQuadrant()
    {
        float shortestDistance = Mathf.Infinity;
        GameObject gazedObject = null;

        if (vrVersion)
        {
            gazedObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            if (gazedObject == null) return Vector3.negativeInfinity;
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Collider>().bounds.IntersectRay(ray))
                {
                    float potentialDistance = Vector3.Distance(cameraObject.transform.position, child.position);
                    if (shortestDistance > potentialDistance)
                    {
                        shortestDistance = potentialDistance;
                        gazedObject = child.gameObject;
                    }
                }
            }
            if (gazedObject == null) return Vector3.negativeInfinity;
        }

        float shortestQuadrantDistance = float.PositiveInfinity;
        Vector3 closestQuadrant = Vector3.zero;
        foreach (Vector3 quadrantCenter in quadrantCenters)
        {
            if (shortestQuadrantDistance > Vector3.Distance(quadrantCenter, gazedObject.transform.position))
            {
                shortestQuadrantDistance = Vector3.Distance(quadrantCenter, gazedObject.transform.position);
                closestQuadrant = quadrantCenter;
            }
        }

        return closestQuadrant;
    }
    
    private Vector3 GetSelectedQuadrant()
    {
        Vector3 selectedQuadrant = GetGazedQuadrant();
        if (selectedQuadrant.x == float.NegativeInfinity) return Vector3.negativeInfinity;
        
        marks = new List<GameObject>();
        foreach (Transform mark in transform)
        {
            if (closestQuadrant(mark.transform.position) != selectedQuadrant)
            {
                marks.Add(mark.gameObject);
                mark.gameObject.SetActive(false);
            }
        }
        confirming = true;
        InitializeConfirmationMenu();
        return selectedQuadrant;
    }

    private void InitializeConfirmationMenu()
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);

        menuObject.transform.parent = transform.root;


        menuObject.transform.localScale = new Vector3(.001f, .001f, .001f);
        if (vrVersion)
        {
            menuObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            /*Vector3 middlePos = transform.root.transform.position + new Vector3(width / 2, height / 2, depth / 2);
            float distance = Vector3.Distance(Camera.main.transform.position, middlePos);
            float ratio = 1.02f * Vector3.Magnitude(new Vector3(width / 2, height / 2, depth / 2)) / distance;


            menuObject.transform.position =  ratio * Camera.main.transform.position + new Vector3(0, 0, -.05f) +
                (1f - ratio) * (transform.root.position + new Vector3(width / 2, height / 2, depth / 2));*/
        }

        Vector3 center = transform.position + new Vector3(width / 2, height / 2, depth / 2);
        float a = 1.05f * Vector3.Magnitude(new Vector3(width / 2, height / 2, depth / 2)) / Vector3.Distance(cameraObject.transform.position, center);
        menuObject.transform.position = a * cameraObject.transform.position + (1 - a) * center;
        menuObject.transform.LookAt(cameraObject.transform);

        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Area?";

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);
    }

    private Vector3 closestQuadrant(Vector3 pos)
    {
        Vector3 closestQuadrant = Vector3.negativeInfinity;
        float closestDistance = Mathf.Infinity;
        foreach (Vector3 quadrant in quadrantCenters) 
        {
            if (Vector3.Magnitude(quadrant - pos) < closestDistance)
            {
                closestDistance = Vector3.Magnitude(quadrant - pos);
                closestQuadrant = quadrant;
            }
        }
        return closestQuadrant;
    }

    private void YesButton()
    {
        HandleTextFile.WriteString("Selection confirmed at " + Time.time);
        float maxVal = quadrantAverages.Values.Max();
        float minVal = quadrantAverages.Values.Min();

        // If the user makes the wrong selection in training, force them to make the right selection
        if (training && ((taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("low") &&
            quadrantAverages[selectedQuadrant] == minVal) ||
            (taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("high") &&
            quadrantAverages[selectedQuadrant] != maxVal)))
        {
            GameObject.Find("TaskHUD").GetComponent<Text>().text = "Incorrect, try again";
            incorrectTime = Time.time + 2;
            NoButton();
            return;
        }

        if (quadrantAverages[selectedQuadrant] == maxVal)
        {
            if (task == "max") HandleTextFile.WriteString("> CORRECT: Selected MAX " + selectedQuadrant + "; Mean=" + quadrantAverages[selectedQuadrant]);
            else HandleTextFile.WriteString("> INCORRECT: Selected MAX " + selectedQuadrant + "; Mean=" + quadrantAverages[selectedQuadrant]);
        }
        else if (quadrantAverages[selectedQuadrant] == minVal)
        {
            if (task == "min") HandleTextFile.WriteString("> CORRECT: Selected MIN" + selectedQuadrant + "; Mean=" + quadrantAverages[selectedQuadrant]);
            else HandleTextFile.WriteString("> INCORRECT: Selected MIN " + selectedQuadrant + "; Mean=" + quadrantAverages[selectedQuadrant]);
        }
        else
        {
            HandleTextFile.WriteString("> INCORRECT Selection " + selectedQuadrant + "; Mean=" + quadrantAverages[selectedQuadrant]);
            HandleTextFile.WriteString("> MIN Delta=" + (quadrantAverages[selectedQuadrant] - minVal) + "; MAX Delta=" + (quadrantAverages[selectedQuadrant] - maxVal));
        }
        if (!GameObject.Find("StudyInfrastructure").GetComponent<StudyInfrastructure>().TrialFinished())
        {
            // Load a message to tell the user they're done

            GameObject CompleteMessagePrefab = null;
            if (vrVersion)
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/VRStudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
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
        // Reset the quadrants 
        foreach (GameObject mark in marks)
        {
            mark.SetActive(true);
        }
        confirming = false;
        Destroy(menuObject);
    }

    private void CalculateAverages()
    {
        List<List<float>> quadrantTotals = new List<List<float>>();
        foreach (Vector3 quad in quadrantCenters)
        {
            quadrantTotals.Add(new List<float>());
        }

        foreach (Transform child in transform)
        {
            Vector3 closestQuad = closestQuadrant(child.transform.position);
            for (var i = 0; i < quadrantCenters.Count; i++)
            {
                if (quadrantCenters[i] == closestQuad)
                {
                    quadrantTotals[i].Add(child.GetComponent<Mark>().GetRealValue());

                    continue;
                }
            }
        }
        quadrantAverages = new Dictionary<Vector3, float>();

        for (var i = 0; i < quadrantTotals.Count; i++)
        {
            List<float> valueSet = quadrantTotals[i];
            float average = valueSet.Count > 0 ? valueSet.Average() : 0.0f;
            Debug.Log(quadrantCenters[i]);
            quadrantAverages.Add(quadrantCenters[i], average);
        }
    }

    public void SetPlane(string plane)
    {
        selectionPlane = plane;
        SetQuadrants();
        CalculateAverages();
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
        if (width > 0) taskDescription.transform.localPosition = new Vector3(width / 2, 1.3f * height, depth / 2) / transform.root.localScale.x;
        taskDescription.transform.localScale = taskDescription.transform.localScale * transform.root.localScale.x;

        taskDescription.transform.Find("Title1").GetComponent<Text>().text = "Select the quadrant with the";
        if (taskName == "min")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "LOWEST";
        }
        else if (taskName == "max")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "HIGHEST";
        }
        taskDescription.transform.Find("TaskSpecs").localPosition = new Vector3(0, -5, 0);
        taskDescription.transform.Find("Title2").GetComponent<Text>().text = "average value";
        task = taskName;
    }
}
