using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerClickListener : MonoBehaviour {

    private SteamVR_TrackedController _controller;
    // Use this for initialization
    void Start () {

        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += HandleTriggerClicked;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (FindObjectsOfType<SelectIndividualMark>().Length > 0) FindObjectsOfType<SelectIndividualMark>()[0].Click();
        else if (FindObjectsOfType<SelectQuadrant>().Length > 0) FindObjectsOfType<SelectQuadrant>()[0].Click();
    }
}
