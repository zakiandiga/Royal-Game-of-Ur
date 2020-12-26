using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshot : MonoBehaviour {

    private int screenshotcount;
    public char Button = 'C';

    // Use this for initialization
    void Start () {
        screenshotcount = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.C))
        {
            ScreenCapture.CaptureScreenshot("Screenshot" + screenshotcount + ".png");
            screenshotcount++;
        }
    }
}
