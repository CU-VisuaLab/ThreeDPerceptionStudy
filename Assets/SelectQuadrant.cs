using DxR;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectQuadrant : MonoBehaviour {
    public string selectionPlane = "XY";
    private float width, height, depth;
    
    private GameObject menuObject;
    private List<GameObject> marks;
    private List<Vector3> quadrantCenters;
    private List<GameObject> quadrants;
    
    private bool confirming;
    private bool clicked;
    private bool vrVersion;

    // Use this for initialization
    void Start () {
        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;
        
        confirming = false;

        clicked = false;
        vrVersion = (GameObject.Find("GazeCursor") != null);

        SetQuadrants();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (vrVersion && clicked)
        {
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
                GetSelectedQuadrant();
            }
            
            clicked = false;
        }
        else if (!vrVersion && Input.GetMouseButtonDown(0))
        {
            GetSelectedQuadrant();
        }
	}

    private void SetQuadrants()
    {
        quadrantCenters = new List<Vector3>();
        quadrants = new List<GameObject>();

        if (selectionPlane == "XY")
        {
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 4, depth / 2));         // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 4, depth / 2));     // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, 3 * height / 4, depth / 2));     // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, 3 * height / 4, depth / 2)); // Top Right
        }
        else if (selectionPlane == "XZ")
        {
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 2, depth / 4));          // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 2, depth / 4));      // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 2, 3 * depth / 4));      // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 2, 3 * depth / 4));  // Top Right
        }
        else if (selectionPlane == "XYZ")
        {
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 4, depth / 4));              // Front Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 4, depth / 4));          // Front Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, 3 * height / 4, depth / 4));          // Front Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, 3 * height / 4, depth / 4));      // Front Top Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 4, 3 * depth / 4));          // Back Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 4, 3 * depth / 4));      // Back Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, 3 * height / 4, 3 * depth / 4));      // Back Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, 3 * height / 4, 3 * depth / 4));  // Back Top Right    
        }
    }
    private Vector3 GetGazedQuadrant()
    {
        float shortestDistance = Mathf.Infinity;
        GameObject gazedObject = null;

        if (vrVersion)
        {
            gazedObject = FindObjectsOfType<GazeCursor>()[0].getHoveredObject();
            Debug.Log(gazedObject);
            if (gazedObject == null) return Vector3.negativeInfinity;
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Collider>().bounds.IntersectRay(ray))
                {
                    float potentialDistance = Vector3.Distance(Camera.main.transform.position, child.position);
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
    
    private void GetSelectedQuadrant()
    {
        Vector3 selectedQuadrant = GetGazedQuadrant();
        if (selectedQuadrant.x == float.NegativeInfinity) return;

        Debug.Log("Selected: " + selectedQuadrant);
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
    }

    private void InitializeConfirmationMenu()
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);


        menuObject.transform.parent = transform.root;
        if (vrVersion)
        {
            menuObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            Vector3 middlePos = transform.root.transform.position + new Vector3(width / 2, height / 2, depth / 2);
            float distance = Vector3.Distance(Camera.main.transform.position, middlePos);
            float ratio = 1.02f * Vector3.Magnitude(new Vector3(width / 2, height / 2, depth / 2)) / distance;


            menuObject.transform.position =  ratio * Camera.main.transform.position + new Vector3(0, 0, -.05f) +
                (1f - ratio) * (transform.root.position + new Vector3(width / 2, height / 2, depth / 2));
            menuObject.transform.LookAt(Camera.main.transform);
        }
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
        Debug.Log(closestQuadrant);
        return closestQuadrant;
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
        foreach (GameObject mark in marks)
        {
            mark.SetActive(true);
        }
        confirming = false;
        Destroy(menuObject);
    }

    public void SetPlane(string plane)
    {
        selectionPlane = plane;
        SetQuadrants();
    }

    // VR Version: Click passed from Vive Controller
    public void Click()
    {
        clicked = true;
    }
}
