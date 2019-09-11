using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROffset : MonoBehaviour {
    private bool initialized = false;
	// Use this for initialization
	void Awake () {
        transform.position = new Vector3(0, -Camera.main.transform.position.y, 0);
        transform.localEulerAngles = new Vector3(0, 110, 0);
    }
	
	// Update is called once per frame
	void Update () {
	}
}
