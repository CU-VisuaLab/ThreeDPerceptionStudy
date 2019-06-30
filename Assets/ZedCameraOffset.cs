using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZedCameraOffset : MonoBehaviour {

    private bool initialized;
	// Use this for initialization
	void Start () {
        initialized = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.Find("Camera_eyes").localPosition != Vector3.zero && !initialized)
        {
            transform.position = -transform.Find("Camera_eyes").localPosition;
      
            transform.localEulerAngles = new Vector3(0, -transform.Find("Camera_eyes").localEulerAngles.y, 0);
            initialized = true;
        }
	}
}
