using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabColliderBehaviour : MonoBehaviour
{
    //This script is attached to DiceGrabCollider (Player dice children)
    DiceBehaviour diceBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        diceBehaviour = GetComponentInParent<DiceBehaviour>();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Hand")
        {
            diceBehaviour.GrabColliderEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Hand")
        {
            diceBehaviour.GrabColliderExit();
        }
    }
}
