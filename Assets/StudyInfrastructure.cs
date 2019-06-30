using DxR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class StudyInfrastructure : MonoBehaviour {

    public int participantNumber = 0;
    private int trialNumber;

    private CSVReader csv;
    private string[,] participantOrderingData;
    private GameObject pleaseWaitMessage;
    private bool loading;
    private int numResets;

    private GameObject cameraObject;

    private Vector3 cameraLastPos;
    private Quaternion cameraLastRot;
    private float distancePos;
    private float distanceRot;

    private bool vrVersion;

    private float taskLoadTime;
    private bool flashingConditions;

    // Use this for initialization
    void Start () {
        HandleTextFile.path = "Assets/Resources/Participant" + participantNumber + ".txt";
        csv = GetComponent<CSVReader>();
        participantOrderingData = CSVReader.SplitCsvGrid(csv.csvFile.text);
        trialNumber = -1;
        numResets = 0;
        vrVersion = (GameObject.Find("GazeCursor") != null);
        if (Camera.main != null) cameraObject = Camera.main.gameObject;
        else cameraObject = GameObject.Find("Camera_eyes");
        flashingConditions = false;
        taskLoadTime = Time.time + 5;
        TrialFinished();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HandleTextFile.WriteString("User Reset to Home at " + Time.time);
            numResets++;
            GameObject.Find("MixedRealityCamera").transform.position = Vector3.zero; 
            GameObject.Find("MixedRealityCamera").transform.localEulerAngles = Vector3.zero;
            cameraLastPos = cameraObject.transform.position;
            cameraLastRot = cameraObject.transform.rotation;
        }
        distancePos += Vector3.Distance(cameraObject.transform.position, cameraLastPos);
        cameraLastPos = cameraObject.transform.position;

        distanceRot += Quaternion.Angle(cameraObject.transform.rotation, cameraLastRot);
        cameraLastRot = cameraObject.transform.rotation;

        if (flashingConditions && Time.time > taskLoadTime)
        {
            LoadTrial();
            flashingConditions = false;
        }
    }

    public bool TrialFinished()
    {
        trialNumber++;
        
        HandleTextFile.WriteString("Positional Distance Traveled: " + distancePos);
        HandleTextFile.WriteString("Rotational Distance Traveled: " + distanceRot);

        HandleTextFile.WriteString("-----------------------------------");

        taskLoadTime = Time.time + 5;
        flashingConditions = true;
        return FlashConditions();
    }

    public bool LoadTrial()
    {
        distancePos = 0;
        distanceRot = 0;

        cameraLastPos = cameraObject.transform.position;
        cameraLastRot = cameraObject.transform.rotation;


        HandleTextFile.WriteString("*** Trial " + trialNumber + " ***");
        try
        {
            // For some reason, the CSVReader class does Col-Row indexing
            string[] parameters = participantOrderingData[trialNumber + 1, participantNumber + 1].Split(' ');
            string prefabName = parameters[0];
            string taskName = parameters[1];
            string taskType = (parameters.Length > 2) ? parameters[2] : "";

            HandleTextFile.WriteString("* Vis: " + prefabName + " Task:  " + taskName + "*");
            GameObject visPrefab = Resources.Load("Prefabs/VisualizationPrefabs/" + prefabName) as GameObject;
            GameObject visObject = Instantiate(visPrefab);
            
            GameObject overviewPrefab = Resources.Load("Prefabs/TaskOverview") as GameObject;
            GameObject overviewObject = Instantiate(overviewPrefab);
            overviewObject.transform.parent = visObject.transform;
            overviewObject.transform.localPosition = new Vector3(-.25f, -.15f, 0);
            GameObject.Find("TaskOverviewText").GetComponent<Text>().text = GameObject.Find("TaskHUD").GetComponent<Text>().text;
            GameObject.Find("TaskHUD").GetComponent<Text>().text = "";

            visObject.transform.Find("DxRAnchor").GetComponent<Renderer>().enabled = false;
            
            GameObject xAxisHead = Instantiate(Resources.Load("Prefabs/AxisArrow") as GameObject);
            xAxisHead.transform.parent = visObject.transform;
            xAxisHead.transform.localPosition = new Vector3(0.51f, 0, 0);
            xAxisHead.transform.localEulerAngles = new Vector3(0, 0, -90);

            GameObject yAxisHead = Instantiate(Resources.Load("Prefabs/AxisArrow") as GameObject);
            yAxisHead.transform.parent = visObject.transform;
            yAxisHead.transform.localPosition = new Vector3(0, 0.51f, 0);
            yAxisHead.transform.localEulerAngles = Vector3.zero;
            Debug.Log(prefabName);
            if (prefabName.Contains("4d") || prefabName.Contains("depth") || prefabName.Contains("height"))
            {
                GameObject zAxisHead = Instantiate(Resources.Load("Prefabs/AxisArrow") as GameObject);
                zAxisHead.transform.parent = visObject.transform;
                zAxisHead.transform.localPosition = new Vector3(0, 0, 0.51f);
                zAxisHead.transform.localEulerAngles = new Vector3(90, 0, 0);
            }
            Transform legendTicks = visObject.transform.Find("DxRView/DxRGuides/Legend(Clone)/Ticks");
            if (legendTicks != null)
            {
                foreach (Transform tick in legendTicks)
                {
                    tick.GetComponent<TextMesh>().text = "";
                }
            }
            
            if (vrVersion)
            {
                if (Camera.main != null)
                {
                    visObject.transform.root.localScale = new Vector3(0.175f, 0.175f, 0.175f);
                    visObject.transform.root.position = new Vector3(-.05f, -.1475f, -.05f);
                }
                else
                {
                    visObject.transform.root.localScale = new Vector3(2, 2, 2);
                    visObject.transform.root.position = new Vector3(-0.5f, -1.475f, -0.5f);
                    visObject.transform.localEulerAngles = new Vector3(0, 0, -90);
                }
            }
            else
            {
                visObject.transform.root.localScale = new Vector3(0.59f, 0.59f, 0.59f);
                visObject.transform.root.position = new Vector3(-.16f, -.19f, .4f);
            }

            if (taskName == "Outlier")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectIndividualMark>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectIndividualMark>().setTask(taskType);
            }
            else if (taskName == "XYQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().SetPlane("XY");
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().setTask(taskType);
            }
            else if (taskName == "XZQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().SetPlane("XZ");
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().setTask(taskType);
            }
            else if (taskName == "XYZQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().SetPlane("XYZ");
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().setTask(taskType);
            }
            else if (taskName == "XYTrend")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectTrend>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().setTask(taskType);
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().SetPlane("XY");
            }
            else if (taskName == "XZTrend")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectTrend>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().setTask(taskType);
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().SetPlane("XZ");
            }
            else if (taskName == "XYZTrend")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectTrend>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().setTask(taskType);
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().SetPlane("XYZ");
            }
            if (GameObject.Find("Zed_Rig_Stereo") != null)
            {
                Debug.Log("Not working??");
                visObject.transform.localScale = new Vector3(2, 2, 2);
            }
            //visObject.GetComponent<Vis>().LoadArrowLegend();
        }
        catch (Exception e)
        {
            loading = false;
            Debug.Log(e.Message);
            HandleTextFile.WriteString("Total Time: " + Time.time);
            HandleTextFile.WriteString("Total Resets: " + numResets);
            return false;
        }
        return true;
    }

    private bool FlashConditions()
    {
        try
        {
            // For some reason, the CSVReader class does Col-Row indexing
            string[] parameters = participantOrderingData[trialNumber + 1, participantNumber + 1].Split(' ');
            string prefabName = parameters[0];
            string taskName = parameters[1];
            string taskType = (parameters.Length > 2) ? parameters[2] : "";

            HandleTextFile.WriteString("* Vis: " + prefabName + " Task:  " + taskName + "*");
            
            string channel = char.ToUpper(prefabName.Split('_')[1][0]) + prefabName.Split('_')[1].Substring(1);
            string dimensionality = prefabName.ToUpper().Contains("3D") ? "2D" : "3D";

            if (taskName.ToLower().Contains("outlier"))
            {
                if (taskType.ToLower().Contains("min")) GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Lowest Value - " + dimensionality;
                else GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Highest Value - " + dimensionality;
            }
            else if (taskName.ToLower().Contains("quad"))
            {
                if (taskType.ToLower().Contains("min")) GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Lowest Average Area - " + dimensionality;
                else GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Highest Average Area - " + dimensionality;
            }
            else
            {
                if (taskType.ToLower().Contains("min")) GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Decreasing Data - " + dimensionality;
                else GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - Increasing Data - " + dimensionality;
            }
            if (GameObject.Find("TaskHUD").GetComponent<Text>().text.Contains("Orient"))
                GameObject.Find("TaskHUD").GetComponent<Text>().text = GameObject.Find("TaskHUD").GetComponent<Text>().text.Replace("Orient", "Orientation");
            //GameObject.Find("TaskHUD").GetComponent<Text>().text = channel + " - " + taskName + " - " + taskType + dimensionality;
            //visObject.GetComponent<Vis>().LoadArrowLegend();
        }
        catch (Exception e)
        {
            loading = false;
            Debug.Log(e.Message);
            HandleTextFile.WriteString("Total Time: " + Time.time);
            HandleTextFile.WriteString("Total Resets: " + numResets);
            return false;
        }
        return true;

    }
}
