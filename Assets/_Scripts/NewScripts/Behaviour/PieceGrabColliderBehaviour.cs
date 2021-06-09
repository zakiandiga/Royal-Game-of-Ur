using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceGrabColliderBehaviour : MonoBehaviour
{
    //This script is attached to DiceGrabCollider (Player dice children)
    PieceBehaviour pieceBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        pieceBehaviour = GetComponentInParent<PieceBehaviour>();
        
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Hand")
        {
            pieceBehaviour.GrabColliderEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Hand")
        {
            pieceBehaviour.GrabColliderExit();
        }
    }
}
