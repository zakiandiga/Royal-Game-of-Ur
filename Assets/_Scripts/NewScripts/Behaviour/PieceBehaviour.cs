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
    public bool hasValidMove = false;
    private bool onFinishSpot = false;
    private int diceResult;

    //private List<int> squareIndexes;
    public int currentSquare = 0;
    public int targetSquare = 0;
    public int finishSquare = 16;
    private int finishSquareIndex = 15;
    private string targetHit;


    #endregion


    [SerializeField] private InputActionReference raycasting; //temp

    #region Event Announcer
    public static event Action<string> OnPieceStateCheck;

    public static event Action<int> OnHoveringPieces;
    public static event Action<PieceBehaviour> OnExitPieceCollider;
    public static event Action<string, bool> OnRaycastHit;
    public static event Action<bool, int> OnPieceDropped;

    public static event Action<bool, bool> OnPieceDropFinalized;

    public static event Action<GameObject> OnPieceFinish;

    public static event Action<string> OnDebugText;

    #endregion

    #region Piece Type,Owner, and State
    [SerializeField] private PieceOwner pieceOwner; //ASSIGN pieceOwner in editor
    public enum PieceOwner
    {
        Player,
        AI
    }

    [SerializeField] private PieceType pieceType; //ASSIGN pieceType in editor
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
        //PhaseManager.OnExitDiceRoll += DiceResultCheck;
        BoardManager.OnLegalMoveAvailable += ReadyingPiece;
        PhaseManager.OnExitPieceMove += PieceMoveConfirmed;
        HandPresence.OnEnterGrip += PieceGrabEnter;
        HandPresence.OnExitGrab += PieceGrabExit;


        raycasting.action.Enable(); //temp
    }

    private void OnDisable()
    {
        //PhaseManager.OnExitDiceRoll -= DiceResultCheck;
        BoardManager.OnLegalMoveAvailable -= ReadyingPiece;
        PhaseManager.OnExitPieceMove -= PieceMoveConfirmed;
        HandPresence.OnEnterGrip -= PieceGrabEnter;
        HandPresence.OnExitGrab -= PieceGrabExit;

        raycasting.action.Disable(); //temp
    }

    #region old check legal move method
    /*
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
        targetSquare = currentSquare + diceResult;
        if(targetSquare > finishSquareIndex) //Temporary. 
        {
            targetSquare = finishSquareIndex;
        }

        pieceState = PieceState.Ready;
        OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER
        //Debug.Log("legal move = " + legalIndex);
    }
    */
    #endregion

    private void ReadyingPiece(int legalMoveAmount)
    {
        if(targetSquare > finishSquareIndex) //Temporary finish square handler
        {
            targetSquare = finishSquareIndex;
        }

        if(hasValidMove || !onFinishSpot)
        {
            pieceState = PieceState.Ready;
            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER (mainly to update UI)
        }
        

    }

    public void GrabColliderEnter()
    {
        if (pieceState == PieceState.Ready)
        {
            //Debug.Log(this.gameObject.name + " is GRABBABLE");


            if (!hasValidMove)
            {
                //notif the player, this.piece has no valid move!
                Debug.Log("This piece has no valid move");
                OnDebugText?.Invoke("This piece has no valid move");
            }

            if (hasValidMove)
            {
                pieceState = PieceState.Grabable;

                OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER
                
                OnHoveringPieces?.Invoke(targetSquare);
                OnDebugText?.Invoke("This piece HAS A VALID MOVE");
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

            OnExitPieceCollider?.Invoke(this); //Tell the board to disable highlight!
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
                

                if(targetHitConvert == targetSquare)
                {
                    targetPosition = hit.transform.position;                    
                    OnRaycastHit?.Invoke(hit.transform.name, true);
                }
                else if (targetHitConvert != targetSquare)
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
        if (targetHit != targetSquare.ToString()) //illegal square drop
        {
            transform.rotation = defaultRotation;
            transform.position = currentPosition; //Move the piece back to the current square

            legalDrop = false;
            Debug.Log("Illegal DROP");

            pieceState = PieceState.Ready;
            OnPieceStateCheck?.Invoke(pieceState.ToString());  //PieceState ANNOUNCER

            //TELL PLAYER THAT THE MOVE IS ILLEGAL (UI and sound)
        }

        else if (targetHit == targetSquare.ToString()) //Legal square drop
        {
            Debug.Log("LEGAL DROP");
            legalDrop = true;

            BoardManager.OnPieceDropHandlerDone += FinalizePieceDrop;

            OnPieceDropped?.Invoke(legalDrop, targetSquare);
            
            //this.grabCollider = false;
        }
    }


    private void FinalizePieceDrop(bool isRosette, bool isKicking, bool isFinish)
    {
        BoardManager.OnPieceDropHandlerDone -= FinalizePieceDrop;

        if (!isKicking)
        {

            if (isFinish)
            {
                //Polish, animate piece movement to goalSpawner
                transform.rotation = defaultRotation;
                transform.position = targetPosition;
                currentPosition = transform.position;

                this.currentSquare = finishSquare;
                pieceState = PieceState.Finished;

                PieceFinishProcession();
            }

            if(!isFinish)
            {
                transform.rotation = defaultRotation;
                transform.position = targetPosition; //Vector3.Lerp(releasePosition, targetPosition, lerpTime * Time.deltaTime);
                currentPosition = transform.position; //Update currentPosition value

                this.currentSquare = targetSquare; //Should check BoardManager for the latest square of this.piece
                OnPieceDropFinalized?.Invoke(legalDrop, isRosette);
            } 
        }

        if(isKicking)
        {
            KickOpponentPiece(isRosette);
        }
    }

    private void KickOpponentPiece(bool isRosette)
    {
        Debug.Log("If opponent's piece is here, kick it!");

        bool isKicking = false;
        FinalizePieceDrop(isRosette, isKicking, false); //redo FinalizePieceDrop after kicking process done
    }

    private void BackToStart()
    {
        //observe OnOpponentAttack() to put piece bact to start position
    }

    private void PieceFinishProcession()
    {
        this.onFinishSpot = true;
        transform.position = goalSpawner.position;
        currentPosition = transform.position;
        OnPieceFinish?.Invoke(this.gameObject);
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

    // Update is called once per frame
    void Update()
    {
        if(raycasting.action.triggered)
        {
            RaycastingTest();
        }

        if(pieceState == PieceState.Waiting)
        {
            if (grabCollider.enabled)
                grabCollider.enabled = false;
        }

        if(pieceState == PieceState.OnHand)
        {
            RaycastingTest();
        }

        if(pieceState == PieceState.Ready && pieceOwner == PieceOwner.Player)
        {
            if (!grabCollider.enabled)
                grabCollider.enabled = true;
        }

        if(pieceState == PieceState.Dropped || onFinishSpot)
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
        //Finish    //Piece landed on the finish spot (exit board)

}
