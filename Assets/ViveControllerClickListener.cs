using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerClickListener : MonoBehaviour {

    private SteamVR_TrackedController _controller;
    // Use this for initialization
    void Start () {

        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += HandleTriggerClicked;
        Debug.Log(_controller);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        Debug.Log("YEAH");
        if (FindObjectsOfType<SelectIndividualMark>().Length > 0) FindObjectsOfType<SelectIndividualMark>()[0].Click();
        else if (FindObjectsOfType<SelectQuadrant>().Length > 0) FindObjectsOfType<SelectQuadrant>()[0].Click();
        else if (FindObjectsOfType<SelectTrend>().Length > 0) FindObjectsOfType<SelectTrend>()[0].Click();
    }
}
