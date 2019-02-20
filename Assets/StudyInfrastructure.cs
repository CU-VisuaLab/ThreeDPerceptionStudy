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
        string prefabName = "";
        try
        {
            prefabName = participantOrderingData[participantNumber + 1, trialNumber + 1];
            GameObject visPrefab = Resources.Load("Prefabs/VisualizationPrefabs/" + prefabName) as GameObject;
            GameObject visObject = Instantiate(visPrefab);
        }
        catch (Exception e)
        {
            loading = false;
            return false;
        }
        return true;
    }
}
