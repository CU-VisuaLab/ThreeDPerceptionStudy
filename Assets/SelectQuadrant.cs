using DxR;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectQuadrant : MonoBehaviour {
    public string selectionPlane = "XY";
    private float width, height, depth;

    private GameObject highlightedObject;
    private GameObject menuObject;
    private List<GameObject> quadrants;

    private Material quadrantSelectionMaterial;
    private Material quadrantInvisibleMaterial;

    private bool selecting, confirming;

	// Use this for initialization
	void Start () {
        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = GetComponent<Vis>().GetVisSize().x / 1000;
        height = GetComponent<Vis>().GetVisSize().y / 1000;
        depth = GetComponent<Vis>().GetVisSize().z / 1000;

        quadrantSelectionMaterial = Resources.Load("Materials/QuadrantSelection") as Material;
        quadrantInvisibleMaterial = Resources.Load("Materials/Invisible") as Material;

        selecting = false;
        confirming = false;

        SetQuadrants();
    }
	
	// Update is called once per frame
	void Update () {
        if (selecting) SetHighlightedQuadrant();
        if (Input.GetKeyDown("r"))
        {
            selecting = true;
            foreach (GameObject quadrant in quadrants)
            {
                quadrant.SetActive(true);
            }
        }
        if (selecting && Input.GetMouseButtonDown(0))
        {
            GetSelectedQuadrant();
        }
	}

    private void SetQuadrants()
    {
        quadrants = new List<GameObject>();
        if (selectionPlane == "XY")
        {
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
                quadrant.GetComponent<Renderer>().material = quadrantInvisibleMaterial;
                quadrant.transform.localScale = new Vector3(width / 2, height / 2, depth);
                quadrant.SetActive(false);
            }
        }
        
    }

    private void SetHighlightedQuadrant()
    {
        if (selectionPlane == "XY")
        {
            highlightedObject = GetGazedQuadrant();
            foreach (GameObject quadrant in quadrants)
            {
                quadrant.GetComponent<Renderer>().material = quadrantInvisibleMaterial;
            }
            if (highlightedObject != null)  highlightedObject.GetComponent<Renderer>().material = quadrantSelectionMaterial;
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
                shortestDistance = Vector3.Distance(Camera.main.transform.position, quadrantObject.transform.position);
                gazedQuadrant = quadrantObject;
            }
        }
        return gazedQuadrant;
    }

    private void GetSelectedQuadrant()
    {
        if (highlightedObject == null) return;

        foreach (GameObject quadrant in quadrants)
        {
            if (quadrant != highlightedObject) quadrant.SetActive(false);
        }
        selecting = false;
        confirming = true;
        InitializeConfirmationMenu(highlightedObject.transform.position);
    }

    private void InitializeConfirmationMenu(Vector3 pos)
    {
        GameObject menuPrefab = Resources.Load("Prefabs/ConfirmationMenu") as GameObject;
        menuObject = GameObject.Instantiate(menuPrefab);

        menuObject.transform.parent = transform;
        menuObject.transform.position = pos + new Vector3(0, 0, -.05f);
        menuObject.transform.Find("Title").GetComponent<Text>().text = "Select this Area?";

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
        // Reset the quadrants 
        foreach (GameObject quadrant in quadrants)
        {
            quadrant.SetActive(true);
        }
        selecting = true;
        confirming = false;
        Destroy(menuObject);
    }
}
