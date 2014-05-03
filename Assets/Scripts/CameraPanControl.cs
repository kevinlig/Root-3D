using UnityEngine;
using System;
using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Gestures.Simple;
using TouchScript.Gestures;
using TouchScript.Events;

public class CameraPanControl : MonoBehaviour {

	private GameObject cameraWrap;

	// Use this for initialization
	void Start () {
		cameraWrap = gameObject.transform.parent.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnEnable() {
		GetComponent<SimplePanGesture>().StateChanged += panHandler;
	}

	void OnDisable() {
		GetComponent<SimplePanGesture>().StateChanged -= panHandler;
	}

	private void panHandler(object sender, GestureStateChangeEventArgs e) {
		// pan detected!
		if (e.State == Gesture.GestureState.Changed) {
        	// list was tapped
        	SimplePanGesture panGesture = GetComponent<SimplePanGesture>();
        	
        	float newX = cameraWrap.transform.position.x - panGesture.WorldDeltaPosition.x;
        	float newZ = cameraWrap.transform.position.z - panGesture.WorldDeltaPosition.y;

        	Vector3 newCameraPos = new Vector3(newX, cameraWrap.transform.position.y, newZ);
        	cameraWrap.transform.position = newCameraPos;
			
        }
	}
}
