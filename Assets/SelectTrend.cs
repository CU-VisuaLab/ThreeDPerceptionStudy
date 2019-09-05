using DxR;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using System.Linq;

public class SelectTrend : MonoBehaviour {
    public string selectionPlane = "XY";
    private float width, height, depth;

    private GameObject menuObject;

    private List<Vector3> directionalPoints;
    private List<GameObject> arrows;

    private bool vrVersion;

    private bool confirming;
    private bool clicked;

    private int correctDirection;
    private double correctFit;
    private int selectedDirection;
    private double[] directionalFits;

    private float startTime;
    private float menuTime;

    private bool orientationVersion;
    private float[] orientationDirectionCounts;
    private GameObject taskDescription;

    private GameObject cameraObject;

    private float incorrectTime;
    private bool training;

    // Use this for initialization
    void Awake()
    {
        if (Camera.main != null) cameraObject = Camera.main.gameObject;
        else cameraObject = GameObject.Find("Camera_eyes");

        vrVersion = (GameObject.Find("GazeCursor") != null);

        // TODO: 1 / 1000 factor defined as a constant in Vis.cs
        width = transform.root.localScale.x * transform.parent.parent.GetComponent<Vis>().GetVisSize().x / 1000;
        height = transform.root.localScale.y * transform.parent.parent.GetComponent<Vis>().GetVisSize().y / 1000;
        depth = transform.root.localScale.z * transform.parent.parent.GetComponent<Vis>().GetVisSize().z / 1000;

        confirming = false;
        
        orientationVersion = false;
        
        training = FindObjectOfType<CSVReader>().csvFile.name.ToLower().Contains("train");
        HandleTextFile.WriteString("Task Loaded at " + Time.time);
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (vrVersion && clicked && confirming)
        {
            clicked = false;
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
        else if ((!confirming && !vrVersion && Input.GetMouseButtonDown(0)) || (vrVersion && clicked))
        {
            selectedDirection = GetSelectedDirection();
            if (selectedDirection >= 0)
            {

                HandleTextFile.WriteString("Selected " + selectedDirection + "; Menu Loaded at " + Time.time);
                HandleTextFile.WriteString("> Time to Completion: " + (Time.time - startTime));
                menuTime = Time.time;
                for (var i = 0; i < arrows.Count; i++)
                {
                    if (i != selectedDirection) arrows[i].SetActive(false);
                }
                InitializeConfirmationMenu(selectedDirection);
            }
            clicked = false;
        }
        else if (clicked && !confirming)
        {
            HandleTextFile.WriteString("Missed click at " + Time.time);
        }

        if (training && GameObject.Find("TaskHUD").GetComponent<Text>().text.Contains("Incorrect") && Time.time > incorrectTime)
        {
            GameObject.Find("TaskHUD").GetComponent<Text>().text = "";
        }
    }

    private void SetDirections()
    {
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

        scaleVector = scaleVector / transform.root.localScale.x;

        if (selectionPlane == "XYZ")
        {
            // For XYZ Trend selection 
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.25f, 0, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.25f, 0, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 1.25f, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, -1.25f, 0)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 0, 1.25f)));
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 0, -1.25f)));
        }
        else
        {
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.25f, 0, 0)));             // East
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.25f, 1.25f, 1.25f)));       // Northeast
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, 1.25f, 1.25f)));          // North
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.25f, 1.25f, 1.25f)));      // Northwest 
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.25f, 0, 0)));            // West
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(-1.25f, -1.25f, -1.25f)));    // Southwest
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(0, -1.25f, -1.25f)));        // South
            directionalPoints.Add(Vector3.Scale(scaleVector, new Vector3(1.25f, -1.25f, -1.25f)));     // Southeast
        }

        for (var i = 0; i < directionalPoints.Count; i++)
        {
            GameObject arrowPrefab = Resources.Load("Prefabs/Arrow") as GameObject;
            GameObject arrow = GameObject.Instantiate(arrowPrefab);
            arrow.transform.parent = transform;
            arrow.transform.localPosition = new Vector3(width / 2, height / 2, depth / 2) / transform.root.localScale.x + directionalPoints[i];
            arrow.transform.localScale = arrow.transform.localScale * transform.root.localScale.x; // FIX THIS!!!!
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

        menuObject.transform.localScale = new Vector3(.001f, .001f, .001f);

        Vector3 center = arrows[directionIndex].transform.position;
        float a = 1.05f * Vector3.Magnitude(new Vector3(width / 2, height / 2, depth / 2)) / Vector3.Distance(cameraObject.transform.position, center);
        menuObject.transform.position = a * cameraObject.transform.position + (1 - a) * center;

        //menuObject.transform.position = arrows[directionIndex].transform.position + new Vector3(0, 0, -.05f);
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
                Debug.Log(hit.transform.name);
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

    private void CorrectDirection()
    {
        int numVals = transform.childCount;
        double[] xVals = new double[numVals];
        double[] yVals = new double[numVals];
        double[] zVals = new double[numVals];
        double[] xPlusYVals = new double[numVals];
        double[] xMinusYVals = new double[numVals];
        double[] realVals = new double[numVals];
        int i = 0;
        foreach (Transform child in transform)
        {
            realVals[i] = child.GetComponent<Mark>().GetRealValue();
            //Debug.Log(realVals[i]);
            xVals[i] = child.localPosition.x;
            if (selectionPlane == "XY")
            {
                yVals[i] = child.localPosition.y;
                xPlusYVals[i] = child.localPosition.x + child.localPosition.y;
                xMinusYVals[i++] = child.localPosition.x - child.localPosition.y;
            }
            else if (selectionPlane == "XZ") // Abstract Z value to get treated as the Y does for XY
            {
                yVals[i] = child.localPosition.z;
                xPlusYVals[i] = child.localPosition.x + child.localPosition.z;
                xMinusYVals[i++] = child.localPosition.x - child.localPosition.z;
            }
            else
            {
                yVals[i] = child.localPosition.y;
                zVals[i++] = child.localPosition.z;
            }
        }
        List<double> fits = new List<double>();
        double xFit = GoodnessOfFit.RSquared(xVals, realVals);
        double yFit = GoodnessOfFit.RSquared(yVals, realVals);
        double zFit = -1;
        double xPlusYFit = -1;
        double xMinusYFit = -1;

        double maxFit = -1;
        if (selectionPlane == "XY" || selectionPlane == "XZ")
        {
            directionalFits = new double[8];
            xPlusYFit = GoodnessOfFit.RSquared(xPlusYVals, realVals);
            xMinusYFit = GoodnessOfFit.RSquared(xMinusYVals, realVals);
            maxFit = new[] { xFit, yFit, xPlusYFit, xMinusYFit }.Max();
        }
        else
        {
            directionalFits = new double[6];
            zFit = GoodnessOfFit.RSquared(zVals, realVals);
            maxFit = new[] { xFit, yFit, zFit }.Max();
        }

        correctDirection = -1;
        if (selectionPlane == "XY" || selectionPlane == "XZ")
        {
            directionalFits[0] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xVals, realVals).Item2) * xFit;
            directionalFits[4] = -directionalFits[0];
            directionalFits[2] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(yVals, realVals).Item2) * yFit;
            directionalFits[6] = -directionalFits[2];
            directionalFits[1] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xPlusYVals, realVals).Item2) * xPlusYFit;
            directionalFits[5] = -directionalFits[1];
            directionalFits[7] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xMinusYVals, realVals).Item2) * xMinusYFit;
            directionalFits[3] = -directionalFits[7];
            
            if (taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("decreasing"))
            {
                for (var index = 0; index < directionalFits.Length; index++)
                {
                    directionalFits[index] *= -1;
                }
            }
            
            if (maxFit == xFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xVals, realVals).Item2 >= 0 ? 0 : 4;
                //correctFit = correctDirection == 0 ? xFit : -xFit; 
            }
            else if (maxFit == yFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(yVals, realVals).Item2 >= 0 ? 2 : 6;
                //correctFit = correctDirection == 2 ? yFit : -yFit;
            }
            else if (maxFit == xPlusYFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xPlusYVals, realVals).Item2 >= 0 ? 1 : 5;
                //correctFit = correctDirection == 1 ? xPlusYFit : -xPlusYFit;
            }
            else
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xMinusYVals, realVals).Item2 >= 0 ? 7 : 3;
                //correctFit = correctDirection == 7 ? xMinusYFit : -xMinusYFit;
            }
            if (taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("decreasing"))
            {
                correctDirection = (correctDirection + 4) % 8;
            }
        }
        else
        {
            directionalFits[0] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xVals, realVals).Item2) * xFit;
            directionalFits[1] = -directionalFits[0];
            directionalFits[2] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(yVals, realVals).Item2) * yFit;
            directionalFits[3] = -directionalFits[2];
            directionalFits[4] = Mathf.Sign((float)MathNet.Numerics.LinearRegression.SimpleRegression.Fit(zVals, realVals).Item2) * zFit;
            directionalFits[5] = -directionalFits[4];

            if (taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("decreasing"))
            {
                for (var index = 0; index < directionalFits.Length; index++)
                {
                    directionalFits[index] *= -1;
                }
            }

            if (maxFit == xFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xVals, realVals).Item2 >= 0 ? 0 : 1;
                //correctFit = correctDirection == 0 ? xFit : -xFit;
            }
            else if (maxFit == yFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(yVals, realVals).Item2 >= 0 ? 2 : 3;
                //correctFit = correctDirection == 2 ? yFit : -yFit;
            }
            else if (maxFit == zFit)
            {
                correctDirection = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(zVals, realVals).Item2 >= 0 ? 4 : 5;
                //correctFit = correctDirection == 4 ? zFit : -zFit;
            }

            if (taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text.ToLower().Contains("decreasing"))
            {
                if (correctDirection % 2 == 0) correctDirection += 1;
                else correctDirection -= 1;
            }
        }
    }

    private void CorrectOrientationDirection()
    {
        if (selectionPlane == "XY")
        {
            orientationDirectionCounts = new float[8];
            foreach (Transform child in transform)
            {
                float val = child.GetComponent<Mark>().GetRealValue();
                for (var i = 0; i < 8; i++)
                {
                    if (Mathf.Abs(val - 45 * i) < 22.5f || Mathf.Abs(val - 360 - 45 * i) < 22.5f)
                    {
                        orientationDirectionCounts[(i + 2) % 8]++;
                    }
                }
            }
            correctFit = orientationDirectionCounts.Max();
            correctDirection = orientationDirectionCounts.ToList().IndexOf((int)correctFit);
        }
    }

    private void YesButton()
    {
        // If the user makes the wrong selection in training, force them to make the right selection
        if (training && selectedDirection != correctDirection)
        {
            GameObject.Find("TaskHUD").GetComponent<Text>().text = "Incorrect, try again";
            incorrectTime = Time.time + 2;
            NoButton();
            return;
        }

        HandleTextFile.WriteString("Selection confirmed at " + Time.time);
        if (selectedDirection == correctDirection) HandleTextFile.WriteString("> CORRECT Direction: " + selectedDirection);
        else if(orientationVersion)
        {
            HandleTextFile.WriteString("Incorrect Direction: Selected=(" + selectedDirection + "," + orientationDirectionCounts[selectedDirection] + "); Correct=(" +
                correctDirection + "," + orientationDirectionCounts[correctDirection] + ")");
            HandleTextFile.WriteString("> Delta: " + (directionalFits[correctDirection] - directionalFits[selectedDirection]) + " (correct - select)");
            if (directionalFits[correctDirection] == -directionalFits[selectedDirection])
                HandleTextFile.WriteString("> Selected OPPOSITE DIRECTION");
        }
        else
        {
            HandleTextFile.WriteString("CORRECT Direction: Selected=(" + selectedDirection + "," + directionalFits[selectedDirection] + "); Correct=(" +
            correctDirection + "," + directionalFits[correctDirection] + ")");
            HandleTextFile.WriteString("> Delta: " + (directionalFits[correctDirection] - directionalFits[selectedDirection]) + " (correct - select)");
            if (directionalFits[correctDirection] == -directionalFits[selectedDirection])
                HandleTextFile.WriteString("> Selected OPPOSITE DIRECTION");
        }
        if (!GameObject.Find("StudyInfrastructure").GetComponent<StudyInfrastructure>().TrialFinished())
        {
            // Load a message to tell the user they're done

            GameObject CompleteMessagePrefab = null;
            if (vrVersion)
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/VRStudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
                //CompleteMessageObject.GetComponent<Canvas>().worldCamera = Camera.main;
            }
            else
            {
                CompleteMessagePrefab = Resources.Load("Prefabs/StudyCompleteMessage") as GameObject;
                GameObject CompleteMessageObject = Instantiate(CompleteMessagePrefab);
            }
        }
        Destroy(transform.root.gameObject);
    }

    public void setTask(string taskName)
    {
        GameObject taskDescriptionPrefab = Resources.Load("Prefabs/TaskDescription") as GameObject;
        taskDescription = Instantiate(taskDescriptionPrefab);
        taskDescription.transform.parent = transform.root;
        if (width > 0) taskDescription.transform.localPosition = new Vector3(width / 2, 1.3f * height, depth / 2) / transform.root.localScale.x;

        taskDescription.transform.Find("Title1").GetComponent<Text>().text = "In which direction is the temperature";

        taskDescription.transform.localScale = taskDescription.transform.localScale * transform.root.localScale.x;

        if (taskName == "max")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "INCREASING";
        }
        else if (taskName == "min")
        {
            taskDescription.transform.Find("TaskSpecs").GetComponent<Text>().text = "DECREASING";
        }
        taskDescription.transform.Find("Title2").GetComponent<Text>().text = "";
    }

    private void NoButton()
    {
        // Reset the arrows

        HandleTextFile.WriteString("Selection rejected at " + Time.time);
        startTime = Time.time;
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
        // UNCOMMENT HERE TO EVALUATE TREND AS MOST ARROWS POINTING THAT WAY
        //orientationVersion = transform.GetChild(0).name.Contains("arrow");
        if (orientationVersion)
        {
            CorrectOrientationDirection();
        }
        else
        {
            CorrectDirection();
        }
        SetDirections();
    }

    // VR Version: Click passed from Vive Controller
    public void Click()
    {
        clicked = true;
    }
}
