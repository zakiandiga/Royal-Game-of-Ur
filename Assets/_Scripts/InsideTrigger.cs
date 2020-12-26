using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideTrigger : MonoBehaviour {

    public string name;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "swallow" || other.tag == "stormbird" || other.tag == "raven" || other.tag == "rooster" || other.tag == "eagle" || other.tag == "dice")
        {
            name = other.name;
            Debug.Log("if:" + other.name);
        }
        else
        {
            Debug.Log("else: " + other.name);
        }
    }
}
