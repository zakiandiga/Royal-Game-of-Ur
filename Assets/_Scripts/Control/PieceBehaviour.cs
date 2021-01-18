using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PieceBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform startSpawner;
    [SerializeField] private Transform goalSpawner;
    private Vector3 currentPosition;
    private float fallTreshold = 0.5f;

    private int diceNumberResult;
    private int diceBoolResult;

    private PieceType pieceType;
    public enum PieceType
    {
        Swallow,
        Stormbird,
        Raven,
        Rooster,
        Eagle
    }

    private PieceState pieceState;
    public enum PieceState
    {
        Waiting,  //Pieces not interactable
        Ready,    //Pieces ready to interact
        Grabable, //Pieces can be grab
        OnHand,   //A piece is currently grabbed
        Dropped,  //Piece is dropped on board
        Finish    //Piece is finish from board
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        currentPosition = startSpawner.position;
    }

    private void OnEnable()
    {
        HandPresence.OnEnterPinch += PieceGrab;
        HandPresence.OnExitPinch += PieceDrop;
        DiceBehaviour.OnDiceNumberResult += DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult += DiceBoolResultCheck;
    }

    private void OnDisable()
    {
        HandPresence.OnEnterPinch -= PieceGrab;
        HandPresence.OnExitPinch -= PieceDrop;
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
    }

    private void DiceNumberResultCheck(int result)
    {
        diceNumberResult = result;
    }

    private void DiceBoolResultCheck(int result)
    {
        diceBoolResult = result;
    }

    private bool CheckLegalMove() //should be on PhaseManager or PlayerManager?
    {
        //check all legal move possibility
        return true; //temporary

    }

    private void PieceGrab(HandPresence hand)
    {
        if(pieceState == PieceState.Grabable)
        {
            pieceState = PieceState.OnHand;
        }
    }

    private void PieceDrop(HandPresence hand)
    {
        if(pieceState == PieceState.OnHand)
        {
            //if move is legal, drop on the selected square, then MovePiece(Vector3 selectedSquare.position)

            //if dropped outside altar, drop on startSpawner (to be handled as dropped not on board)

            //if move is not legal or if dropped not on board, drop on current position
            

            pieceState = PieceState.Dropped;
            currentPosition = transform.position;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Hand")
        {
            if(pieceState == PieceState.Ready)
            {
                pieceState = PieceState.Grabable;
                Debug.Log(this.gameObject.name + " is GRABBABLE");
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Hand")
        {
            if (pieceState == PieceState.Grabable)
            {
                pieceState = PieceState.Ready;
                Debug.Log(this.gameObject.name + " is NOT GRABBABLE");
            }
        }
    }

    private void MovePiece(Vector3 selectedSquare)
    {
        Vector3 destination = selectedSquare;
        transform.position = destination; //Temporary

        //StartCoroutine(PieceMovement(destination));  //Polishing backlog, to animate the piece movement
    }

    private IEnumerator PieceMovement(Vector3 destination)
    {
        float delay = 0.5f;
        yield return new WaitForSeconds(delay);
        //move piece per square
    }

    private void BackToStart()
    {
        //observe OnOpponentAttack() to put piece bact to start position
    }

    // Update is called once per frame
    void Update()
    {
        if(diceBoolResult > 0 && diceNumberResult > 0 && pieceState == PieceState.Waiting) //need to consider if the player have valid move
        {
            if(CheckLegalMove())
            {
                pieceState = PieceState.Ready;
            }            
 
        }

    }
}
