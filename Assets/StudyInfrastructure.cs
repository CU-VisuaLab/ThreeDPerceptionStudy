using DxR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyInfrastructure : MonoBehaviour {

    public int participantNumber = 0;
    private int trialNumber;

    private CSVReader csv;
    private string[,] participantOrderingData;
    private GameObject pleaseWaitMessage;
    private bool loading;
    private int numResets;

    // Use this for initialization
    void Start () {
        HandleTextFile.path = "Assets/Resources/Participant" + participantNumber + ".txt";
        csv = GetComponent<CSVReader>();
        participantOrderingData = CSVReader.SplitCsvGrid(csv.csvFile.text);
        trialNumber = -1;
        numResets = 0;
        LoadTrial();
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
        }
    }

    public bool LoadTrial()
    {
        trialNumber++;
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

            if (taskName == "Outlier")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectIndividualMark>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectIndividualMark>().setTask(taskType);
            }
            else if (taskName == "XYQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
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
            }
            else if (taskName == "XZTrend")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectTrend>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().SetPlane("XZ");
            }
            else if (taskName == "XYZTrend")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectTrend>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectTrend>().SetPlane("XYZ");
            }
            visObject.GetComponent<Vis>().LoadArrowLegend();
        }
        catch (Exception e)
        {
            loading = false;
            Debug.Log(e.Message);
            HandleTextFile.WriteString("Total Resets: " + numResets);
            HandleTextFile.WriteString("Total Time: " + Time.time);
            return false;
        }
        return true;
    }
}
