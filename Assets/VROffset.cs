using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROffset : MonoBehaviour {
    private bool initialized = false;
	// Use this for initialization
	void Awake () {
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized && GetComponentInChildren<Camera>().transform.position != Vector3.zero)
        {
            initialized = true;
            transform.position = new Vector3(0, -GetComponentInChildren<Camera>().transform.position.y / 10, 0);
        }
	}
}
