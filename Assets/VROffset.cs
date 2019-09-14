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
        if (!initialized)
        {
            initialized = true;
            transform.position = new Vector3(-.075f, .01f, 0);
            transform.localEulerAngles = new Vector3(0, 30, 0);
        }
	}
}
