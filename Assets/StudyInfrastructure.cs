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

    // Use this for initialization
    void Start () {
        csv = GetComponent<CSVReader>();
        participantOrderingData = CSVReader.SplitCsvGrid(csv.csvFile.text);
        trialNumber = -1;
        LoadTrial();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public bool LoadTrial()
    {
        trialNumber++;
        try
        {
            // For some reason, the CSVReader class does Col-Row indexing
            string prefabName = participantOrderingData[trialNumber + 1, participantNumber + 1].Split(' ')[0];
            string taskName = participantOrderingData[trialNumber + 1, participantNumber + 1].Split(' ')[1];

            Debug.Log(prefabName);
            Debug.Log(taskName);

            GameObject visPrefab = Resources.Load("Prefabs/VisualizationPrefabs/" + prefabName) as GameObject;
            GameObject visObject = Instantiate(visPrefab);

            if (taskName == "Outlier")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectIndividualMark>();
            }
            else if (taskName == "XYQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
            }
            else if (taskName == "XZQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().SetPlane("XZ");
            }
            else if (taskName == "XYZQuadrant")
            {
                visObject.transform.Find("DxRView/DxRMarks").gameObject.AddComponent<SelectQuadrant>();
                visObject.transform.Find("DxRView/DxRMarks").GetComponent<SelectQuadrant>().SetPlane("XYZ");
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
        }
        catch (Exception e)
        {
            loading = false;
            return false;
        }
        return true;
    }
}
