using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROffset : MonoBehaviour {
    private bool initialized = false;
	// Use this for initialization
	void Awake () {
        transform.position = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
