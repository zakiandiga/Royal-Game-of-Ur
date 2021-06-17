using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceGrabColliderBehaviour : MonoBehaviour
{
    //This script is attached to DiceGrabCollider (Player dice children)
    private PieceBehaviour pieceBehaviour;
    private GameObject piece;
    private GameObject currentGrabbable = null;
    private bool isGrabbable = false;

    // Start is called before the first frame update
    void Start()
    {
        piece = transform.parent.gameObject;
        pieceBehaviour = piece.GetComponent<PieceBehaviour>();       
    }

    private void OnEnable()
    {
        PieceBehaviour.OnPieceStateCheck += LatestGrabbableCheck;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnPieceStateCheck -= LatestGrabbableCheck;
    }

    //Handling enter a new grabCollider while the current grabCollider does not exit
    private void LatestGrabbableCheck(GameObject collidingPiece, string pieceState)
    {
        currentGrabbable = collidingPiece;
        if(this.piece != currentGrabbable && this.isGrabbable)
        {
            this.isGrabbable = false;
            this.pieceBehaviour.GrabColliderExit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Hand")
        {
            this.isGrabbable = true;
            pieceBehaviour.GrabColliderEnter();            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Hand")
        {
            this.isGrabbable = false;
            pieceBehaviour.GrabColliderExit();
        }
    }
}
