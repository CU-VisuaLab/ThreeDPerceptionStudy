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

	// Use this for initialization
	void Start () {
        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;
        
        confirming = false;

        SetQuadrants();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (confirming) return;
        if (Input.GetMouseButtonDown(0))
        {
            GetSelectedQuadrant();
        }
	}

    private void SetQuadrants()
    {
        if (quadrants != null)
        {
            foreach (GameObject quad in quadrants)
            {
                Destroy(quad);
            }
        }
        quadrantCenters = new List<Vector3>();
        quadrants = new List<GameObject>();

        if (selectionPlane == "XY")
        {
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 4, depth / 2));          // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 4, depth / 2));      // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, 3 * height / 4, depth / 2));      // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, 3 * height / 4, depth / 2)); // Top Right

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[0].transform.parent = transform;
            quadrants[0].transform.name = "BottomLeft";
            quadrants[0].transform.localPosition = new Vector3(width / 4, height / 4, depth / 2);

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[1].transform.parent = transform;
            quadrants[1].transform.name = "BottomRight";
            quadrants[1].transform.localPosition = new Vector3(3 * width / 4, height / 4, depth / 2);

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[2].transform.parent = transform;
            quadrants[2].transform.name = "TopLeft";
            quadrants[2].transform.localPosition = new Vector3(width / 4, 3 * height / 4, depth / 2);

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[3].transform.parent = transform;
            quadrants[3].transform.name = "TopRight";
            quadrants[3].transform.localPosition = new Vector3(3 * width / 4, 3 * height / 4, depth / 2);

            foreach (GameObject quadrant in quadrants)
            {
                quadrant.GetComponent<Renderer>().enabled = false;
                quadrant.transform.localScale = new Vector3(width / 2, height / 2, depth);
            }
        }
        else if (selectionPlane == "XZ")
        {
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 2, depth / 4));          // Bottom Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 2, depth / 4));      // Bottom Right
            quadrantCenters.Add(transform.position + new Vector3(width / 4, height / 2, 3 * depth / 4));      // Top Left
            quadrantCenters.Add(transform.position + new Vector3(3 * width / 4, height / 2, 3 * depth / 4)); // Top Right

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[0].transform.parent = transform;
            quadrants[0].transform.name = "BottomLeft";
            quadrants[0].transform.position = quadrantCenters[0];

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[1].transform.parent = transform;
            quadrants[1].transform.name = "BottomRight";
            quadrants[1].transform.position = quadrantCenters[1];

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[2].transform.parent = transform;
            quadrants[2].transform.name = "TopLeft";
            quadrants[2].transform.position = quadrantCenters[2];

            quadrants.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            quadrants[3].transform.parent = transform;
            quadrants[3].transform.name = "TopRight";
            quadrants[3].transform.position = quadrantCenters[3];

            foreach (GameObject quadrant in quadrants)
            {
                quadrant.GetComponent<Renderer>().enabled = false;
                quadrant.transform.localScale = new Vector3(width / 2, height, depth / 2);
            }
        }
        
    }
    private GameObject GetGazedQuadrant()
    {
        float shortestDistance = Mathf.Infinity;
        GameObject gazedQuadrant = null;
        
        foreach (GameObject quadrantObject in quadrants)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (quadrantObject.GetComponent<Collider>().bounds.IntersectRay(ray))
            {
                float potentialDistance = Vector3.Distance(Camera.main.transform.position, quadrantObject.transform.position);
                if (shortestDistance > potentialDistance)
                {
                    shortestDistance = potentialDistance;
                    gazedQuadrant = quadrantObject;
                }
            }
        }
        return gazedQuadrant;
    }
    
    private void GetSelectedQuadrant()
    {
        GameObject selectedQuadrant = GetGazedQuadrant();
        if (selectedQuadrant == null) return;
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
        InitializeConfirmationMenu(selectedQuadrant.transform.position);
    }

    private void InitializeConfirmationMenu(Vector3 pos)
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);

        menuObject.transform.parent = transform.root;
        menuObject.transform.position = pos + new Vector3(0, 0, -.05f);
        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Area?";

        menuObject.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(YesButton);

        menuObject.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(NoButton);
    }

    private GameObject closestQuadrant(Vector3 pos)
    {
        GameObject closestQuadrant = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject quadrant in quadrants) 
        {
            if (Vector3.Magnitude(quadrant.transform.position - pos) < closestDistance)
            {
                closestDistance = Vector3.Magnitude(quadrant.transform.position - pos);
                closestQuadrant = quadrant;
            }
        }
        return closestQuadrant;
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
}
