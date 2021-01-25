﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class PieceBehaviour : MonoBehaviour
{
    #region PhysicalProperty
    private Rigidbody rb;
    private SphereCollider grabCollider;
    [SerializeField] private Transform startSpawner;
    [SerializeField] private Transform goalSpawner;

    private float fallTreshold = 0.5f;

    private Vector3 currentPosition;
    private Vector3 targetPosition; //vector3 of piece's landing snap position
    private Quaternion defaultRotation;
    [SerializeField] private LayerMask pieceHitMask;
    #endregion

    #region MovementProperty
    private bool legalDrop = false;
    private bool hasValidMove = false;
    private bool isFinish = false;
    private int diceResult;

    //private List<int> squareIndexes;
    private int currentSquare = 0;
    private int legalIndex = 0;
    private int finishSquareIndex = 15;
    private string targetHit;


    #endregion


    [SerializeField] private InputActionReference raycasting; //temp

    #region Event Announcer
    public static event Action<string> OnPieceStateCheck;

    public static event Action<int> OnMoveValidCheck;
    public static event Action<PieceBehaviour> OnExitPieceCollider;
    public static event Action<string, bool> OnRaycastHit;
    public static event Action<bool> OnPieceDropped;

    public static event Action<GameObject> OnPieceFinish;

    #endregion

    #region Piece Type,Owner, and State
    [SerializeField] private PieceOwner pieceOwner; //ASSIGN pieceOwner ON EDITOR
    public enum PieceOwner
    {
        Player,
        AI
    }

    [SerializeField] private PieceType pieceType; //ASSIGN pieceType ON EDITOR
    public enum PieceType
    {
        Swallow,
        Stormbird,
        Raven,
        Rooster,
        Eagle,
    }

    private PieceState pieceState;
    public enum PieceState
    {
        Waiting,  //Pieces not interactable
        Ready,    //Pieces ready to interact
        Grabable, //Pieces can be grab
        OnHand,   //A piece is currently grabbed
        Dropped,  //Piece is dropped on board
        Finished
        
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {    
        rb = GetComponent<Rigidbody>();
        grabCollider = GetComponentInChildren<SphereCollider>();

        grabCollider.enabled = false;
        
        currentPosition = startSpawner.position;
        defaultRotation = transform.rotation;

        //Testing roll dice
        //currentSquare = 2;
        //diceResult = 3;
        //CheckLegalMove();

        pieceState = PieceState.Dropped;
        OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

    }

    private void OnEnable()
    {
        PhaseManager.OnExitDiceRoll += DiceResultCheck;
        PhaseManager.OnExitPieceMove += PieceMoveConfirmed;
        HandPresence.OnEnterGrip += PieceGrabEnter;
        HandPresence.OnExitGrab += PieceGrabExit;


        raycasting.action.Enable(); //temp
    }

    private void OnDisable()
    {
        PhaseManager.OnExitDiceRoll -= DiceResultCheck;

        raycasting.action.Disable(); //temp
    }

    private void DiceResultCheck(int result)
    {
        diceResult = result;

        CheckLegalMove();
    }

    private void CheckOccupiedSquares()
    {

        
    }
       
    private void CheckLegalMove() //should be on PhaseManager or PlayerManager?
    {        
        legalIndex = currentSquare + diceResult;
        if(legalIndex > finishSquareIndex)
        {
            legalIndex = finishSquareIndex;
        }

        pieceState = PieceState.Ready;
        OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER
        //Debug.Log("legal move = " + legalIndex);

    }

    public void GrabColliderEnter()
    {
        if (pieceState == PieceState.Ready)
        {
            pieceState = PieceState.Grabable;

            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

            //Debug.Log(this.gameObject.name + " is GRABBABLE");

            OnMoveValidCheck?.Invoke(legalIndex); //Test, suppost to be on !hasValidMove

            if (hasValidMove == false)
            {
                //notif the player, this.piece has no valid move!
            }

            else
            {
                //'Tell' board (PhaseManager) to green higlight legal square for this.piece
                
            }                               
        }
    }

    public void GrabColliderExit()
    {
        if (pieceState != PieceState.OnHand)
        {
            pieceState = PieceState.Ready;
            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

            //Debug.Log(this.gameObject.name + " is NOT GRABBABLE");

            OnExitPieceCollider?.Invoke(this);
            //Tell the board to disable highlight!
        }
    }

    public void PieceGrabEnter(HandPresence hand)
    {
        //tell the board to turn off highlight

        if (pieceState == PieceState.Grabable)
        {
            pieceState = PieceState.OnHand;
            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

        }
    }

    private void RaycastingTest() //Triggered on Update()
    {
        float range = 500f;
        targetHit = null;
        int targetHitConvert;
        RaycastHit previousHit;

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(transform.position, Vector3.down, Color.red, 1, true);
        if (Physics.Raycast(ray, out hit, range, pieceHitMask))
        {

            if (targetHit == null || targetHit != hit.transform.name)
            {
                targetHit = hit.transform.name;
                int.TryParse(targetHit, out targetHitConvert);
                //Debug.Log("Hit on " + targetHitConvert);
                

                if(targetHitConvert == legalIndex)
                {
                    targetPosition = hit.transform.position;                    
                    OnRaycastHit?.Invoke(hit.transform.name, true);
                }
                else if (targetHitConvert != legalIndex)
                {
                    OnRaycastHit?.Invoke(hit.transform.name, false);
                }

            }

            previousHit = hit;

        }
    }

    public void PieceGrabExit(HandPresence hand)
    {
        
        //Landing at square handler
        //float lerpTime = 0.5f;
        if(pieceState == PieceState.OnHand)
        {
            //Vector3 releasePosition = transform.position;
            DropPiece();
        }        
    }
        
    private void DropPiece()
    {
        if (targetHit != legalIndex.ToString()) //illegal square drop
        {
            transform.rotation = defaultRotation;
            transform.position = currentPosition;

            legalDrop = false;
            Debug.Log("Illegal DROP");

            pieceState = PieceState.Ready;
            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

            //TELL PLAYER THAT THE MOVE IS ILLEGAL (UI and sound)
        }

        else if (targetHit == legalIndex.ToString()) //Legal square drop
        {
            transform.rotation = defaultRotation;

            transform.position = targetPosition; //Vector3.Lerp(releasePosition, targetPosition, lerpTime * Time.deltaTime);

            currentPosition = transform.position; //Update currentPosition value

            Debug.Log("LEGAL DROP");
            legalDrop = true;            

            if(targetHit == finishSquareIndex.ToString())
            {
                this.isFinish = true;

                //Polish, animate piece movement to goalSpawner
                transform.position = goalSpawner.position;
                pieceState = PieceState.Finished;

                OnPieceFinish?.Invoke(this.gameObject);
            }

            this.currentSquare = legalIndex;
            OnPieceDropped?.Invoke(legalDrop);

            KickOpponentPiece(); 
            
            //this.grabCollider = false;
        }
    }

    private void KickOpponentPiece()
    {
        Debug.Log("If opponent's piece is here, kick it!");
    }

    private void PieceMoveConfirmed(PhaseManager phase)
    {
        pieceState = PieceState.Dropped;
        OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER
    }

    private void MovePiece(Vector3 selectedSquare)
    {
        Vector3 destination = selectedSquare;
        transform.position = destination; //Temporary

        //StartCoroutine(PieceMovement(destination));  
        //Polishing backlog, to animate the piece movement
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
        if(raycasting.action.triggered)
        {
            RaycastingTest();
        }

        if(pieceState == PieceState.OnHand)
        {
            RaycastingTest();
        }

        if(pieceState == PieceState.Ready)
        {
            if (!grabCollider.enabled)
                grabCollider.enabled = true;
        }

        if(pieceState == PieceState.Dropped)
        {
            if (grabCollider.enabled)
                grabCollider.enabled = false;
        }


    }

        //Waiting,  //Pieces not interactable
        //Ready,    //Pieces ready to interact
        //Grabable, //Pieces can be grab
        //OnHand,   //A piece is currently grabbed
        //Dropped,  //Piece is dropped on board
        //Finish

}
