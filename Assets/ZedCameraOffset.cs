﻿using System.Collections;
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
            transform.position = Vector3.zero;// new Vector3(-transform.Find("Camera_eyes").localPosition.x,0, -transform.Find("Camera_eyes").localPosition.z);

            GetComponent<ZEDManager>().depthMode = sl.DEPTH_MODE.PERFORMANCE;
            transform.localEulerAngles =  new Vector3(0, 110, 0);
            initialized = true;
        }
	}
}
