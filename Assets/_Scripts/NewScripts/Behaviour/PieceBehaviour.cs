using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class PieceBehaviour : MonoBehaviour
{
    #region PhysicalProperty
    private PieceGrabInteractable grabControl;
    private Rigidbody rb;
    private BoxCollider pieceCollider;
    private SphereCollider grabCollider;
    [SerializeField] private LayerMask interactableOn;
    [SerializeField] private LayerMask interactableOff;

    [SerializeField] private Transform startSpawner;
    [SerializeField] private Transform goalSpawner;

    private float fallTreshold = 0.5f;

    private Vector3 currentPosition;
    private Vector3 targetPosition; //vector3 of piece's landing snap position
    private Quaternion defaultRotation;
    [SerializeField] private LayerMask pieceHitMask;
    [SerializeField] private LayerMask opponentHitMask;
    #endregion

    #region MovementProperty
    private bool legalDrop = false;
    public bool hasValidMove = false;
    private bool onStartingSpot = true;
    private bool onFinishSpot = false;
    private int diceResult;
    public bool hasLaunched = false;

    //private List<int> squareIndexes;
    public int currentSquare = 0;
    public int playerTargetSquare = 0;
    public int aiTargetSquare = 0;
    public int finishSquare = 16;
    private int finishSquareIndex = 15;
    private string targetHit;
    #endregion


    [SerializeField] private InputActionReference raycasting; //temp DEBUG

    #region Event Announcer
    public static event Action<GameObject, string> OnPieceStateCheck;

    public static event Action<int> OnHoveringPieces;
    public static event Action<PieceBehaviour> OnExitPieceCollider;
    public static event Action<string, bool> OnRaycastHit;

    public static event Action<bool, int, bool> OnPieceDropped;
    public static event Action<GameObject> OnPieceBackToStart;
    public static event Action<bool, bool, bool> OnPieceDropFinalized;
    public static event Action<GameObject, bool> OnPieceFinish;

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

    public PieceState pieceState = PieceState.Waiting; //switched to public for existing AI handle
    public enum PieceState
    {
        Waiting,  //Pieces not interactable
        Ready,    //Pieces ready to interact
        Grabable, //Pieces can be grab
        OnHand,   //A piece is currently grabbed
        Dropped,  //Piece is dropped on board
        AIMoving, //Piece is being moved by the AI (On the AI hand)
        AIDroped, //Piece is dropped by the AI
        Finished  //Piece entered the finish spot
        
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        grabControl = GetComponent<PieceGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        pieceCollider = GetComponent<BoxCollider>();
        grabCollider = GetComponentInChildren<SphereCollider>();
     
        currentPosition = startSpawner.position;
        defaultRotation = transform.rotation;

        //Testing roll dice
        //currentSquare = 2;
        //diceResult = 3;
        //CheckLegalMove();

        pieceState = PieceState.Waiting;
        OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER

    }

    private void OnEnable()
    {
        //PhaseManager.OnExitDiceRoll += DiceResultCheck;
        if(pieceOwner == PieceOwner.Player)
        {
            BoardManager.OnLegalMoveAvailable += ReadyingPiece;
            HandPresence.OnEnterGrip += PieceGrabEnter;
            HandPresence.OnExitGrab += PieceGrabExit;
        }

        else if (pieceOwner == PieceOwner.AI)
        {

        }

        raycasting.action.Enable(); //temp debug, testing raycast with button
    }

    private void OnDisable()
    {
        //PhaseManager.OnExitDiceRoll -= DiceResultCheck;
        if(pieceOwner == PieceOwner.Player)
        {
            BoardManager.OnLegalMoveAvailable -= ReadyingPiece;
            HandPresence.OnEnterGrip -= PieceGrabEnter;
            HandPresence.OnExitGrab -= PieceGrabExit;
        }

        else if (pieceOwner == PieceOwner.AI)
        {

        }

        raycasting.action.Disable(); //temp debug, testing raycast with button
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

    #region AIMovementFunctions
    public void AIMovePiece(int destination)
    {
        if (pieceOwner == PieceOwner.AI)
        {
            if (pieceState != PieceState.AIMoving)
            {
                this.rb.isKinematic = true;
                aiTargetSquare = destination;
                Debug.Log(gameObject.name + " is being moved by the AI");
                pieceState = PieceState.AIMoving;
                //AIAnimationStateMachine.OnPieceDropAI += PieceDropAI;

            }
        }
    }

    public void AIDropPiece()
    {
        if (pieceOwner == PieceOwner.AI)
        {

            if (pieceState != PieceState.AIDroped)
            {
                Debug.Log("PieceBehaviour: AIDropPiece() called");
                DropPiece();
                pieceState = PieceState.AIDroped;
            }
        }
    }        
    #endregion

    private void ReadyingPiece(int legalMoveAmount)
    {
        PhaseManager.OnExitPieceMove += PieceMoveConfirmed;

        if (legalMoveAmount <= 0)
        {
            if(!onFinishSpot)
            {
                if (pieceState != PieceState.Waiting)
                    pieceState = PieceState.Waiting;
            }

            if (onFinishSpot)
            {
                hasValidMove = false;
                if (pieceState != PieceState.Finished)
                    pieceState = PieceState.Finished;
            }
        }

        if(legalMoveAmount > 0)
        {
            if (playerTargetSquare > finishSquareIndex) //Temporary finish square handler
            {
                playerTargetSquare = finishSquareIndex;
            }

            if (hasValidMove || !onFinishSpot)
            {
                pieceState = PieceState.Ready;
                OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER (mainly to update UI)
            }

            if (onFinishSpot)
            {
                hasValidMove = false;
                if (pieceState != PieceState.Finished)
                    pieceState = PieceState.Finished;
            }

            if (!hasValidMove)
            {
                if (pieceState != PieceState.Waiting)
                    pieceState = PieceState.Waiting;
            }
        }     
    }

    public void GrabColliderEnter()
    {
        if (pieceState == PieceState.Ready)
        {
            if (!hasValidMove)
            {
                //notif the player, this.piece has no valid move!
                Debug.Log("This piece has no valid move");
                OnDebugText?.Invoke("This piece has no valid move");
            }

            if (hasValidMove)
            {
                pieceState = PieceState.Grabable;

                OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER
                
                OnHoveringPieces?.Invoke(playerTargetSquare);
                OnDebugText?.Invoke("This piece HAS A VALID MOVE");
                //'Tell' board (PhaseManager) to green higlight legal square for this.piece
            }                               
        }
    }

    public void GrabColliderExit()
    {
        if (pieceState == PieceState.Grabable)
        {
            pieceState = PieceState.Ready;
            //OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER

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
            if(pieceState == PieceState.OnHand && pieceCollider.enabled)            
            {
                pieceCollider.enabled = false;
            }

            OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER

        }
    }

    private void BoardCheckRaycast() //Triggered on Update()
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
            if(pieceOwner == PieceOwner.Player)
            {
                if (targetHit == null || targetHit != hit.transform.name)
                {
                    targetHit = hit.transform.name;
                    int.TryParse(targetHit, out targetHitConvert);
                    //Debug.Log("Hit on " + targetHitConvert);


                    if (targetHitConvert == playerTargetSquare)
                    {
                        targetPosition = hit.transform.position;
                        OnRaycastHit?.Invoke(hit.transform.name, true);
                    }
                    else if (targetHitConvert != playerTargetSquare)
                    {
                        OnRaycastHit?.Invoke(hit.transform.name, false);
                    }

                }
            }

            if(pieceOwner == PieceOwner.AI)
            {
                targetPosition = hit.transform.position;
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
            if (!pieceCollider.enabled)
                pieceCollider.enabled = true;

            DropPiece();
        }        
    }
        
    private void DropPiece()
    {
        bool isPlayerPiece;
        if(pieceOwner == PieceOwner.AI)
        {
            //AI piecedrop handling
            //assume AI piecedrop always legal (handled by AI script)
            isPlayerPiece = false;
            legalDrop = true; //legalDrop from AI always true
            //this.rb.isKinematic = false;
            BoardManager.OnPieceDropHandlerDone += FinalizePieceDrop;
            OnPieceDropped?.Invoke(legalDrop, aiTargetSquare, isPlayerPiece);
        }

        else if(pieceOwner == PieceOwner.Player)
        {
            isPlayerPiece = true;
            if (targetHit != playerTargetSquare.ToString()) //illegal square drop
            {
                transform.rotation = defaultRotation;
                transform.position = currentPosition; //Move the piece back to the current square

                legalDrop = false;
                Debug.Log("Illegal DROP");

                pieceState = PieceState.Ready;
                OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER

                //TELL PLAYER THAT THE MOVE IS ILLEGAL (UI and sound)
            }

            else if (targetHit == playerTargetSquare.ToString()) //Legal square drop
            {
                Debug.Log("LEGAL DROP");
                legalDrop = true;

                BoardManager.OnPieceDropHandlerDone += FinalizePieceDrop;

                OnPieceDropped?.Invoke(legalDrop, playerTargetSquare, isPlayerPiece);

                //this.grabCollider = false;
            }
        }
        
    }


    private void FinalizePieceDrop(bool isRosette, bool isKicking, bool isFinish)
    {
        BoardManager.OnPieceDropHandlerDone -= FinalizePieceDrop;
        bool isPlayerPiece;

        if (!isKicking)
        {
            if (isFinish) //finish drop
            {
                //Polish, animate piece movement to goalSpawner
                transform.rotation = defaultRotation;
                transform.position = targetPosition;
                currentPosition = transform.position;

                this.currentSquare = finishSquare;                

                PieceFinishProcession();
            }

            if(!isFinish) //regular drop
            {
                transform.rotation = defaultRotation;
                transform.position = targetPosition; //Vector3.Lerp(releasePosition, targetPosition, lerpTime * Time.deltaTime);
                currentPosition = transform.position; //Update currentPosition value


                if (pieceOwner == PieceOwner.Player)
                {
                    this.currentSquare = playerTargetSquare; //Should check BoardManager for the latest square of this.piece
                    isPlayerPiece = true;
                    Debug.Log("Regular drop executed (player)");
                    OnPieceDropFinalized?.Invoke(legalDrop, isRosette, isPlayerPiece);
                }
                else if (pieceOwner == PieceOwner.AI)
                {
                    //Tell AIAnimationStateMachine AI_State to switch to S_IKtoWAIT
                    //pass isRosette = false info
                    this.currentSquare = aiTargetSquare;
                    isPlayerPiece = false;
                    Debug.Log("Regular drop executed (AI)");
                    OnPieceDropFinalized?.Invoke(legalDrop, isRosette, isPlayerPiece);
                    Debug.Log("PieceBehaviour: AI Piece finishing piecedrop");
                }
                
            } 
        }

        else if(isKicking) //kicking drop
        {
            KickOpponentPiece(isRosette);
        }
    }

    private void KickOpponentPiece(bool isRosette)
    {
        Debug.Log("If opponent's piece is here, kick it!");

        float range = 500f;
        GameObject hitResult = null;
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, opponentHitMask))
        {
            hitResult = hit.collider.gameObject;
            PieceBehaviour opponentPiece = hitResult.GetComponent<PieceBehaviour>();
            OnDebugText?.Invoke("Kick a piece: " + hitResult.name);

            Debug.Log("PieceBehaviour: Kicking " + opponentPiece.gameObject.name);
            if (hitResult != null)
            {
                opponentPiece.BackToStart();
            }
        }

        bool isKicking = false;
        FinalizePieceDrop(isRosette, isKicking, false); //redo FinalizePieceDrop after kicking process done
        //Finalize after kicking always pass false isFinish argument (isFinish piece never kick opponent pieces)
    }

    private void BackToStart()
    {
        //triggered from KickOpponentPiece()
        transform.position = this.startSpawner.position;
        currentPosition = transform.position;
        this.currentSquare = 0;
        OnPieceBackToStart?.Invoke(this.gameObject);
                
        //Polish notes: 
        //Animate the kicking sequence
        //Tell UI to show the info
    }

    private void PieceFinishProcession()
    {
        bool playerPiece;
        pieceState = PieceState.Finished;
        this.onFinishSpot = true;
        transform.position = goalSpawner.position;
        currentPosition = transform.position; //Lock the piece position on the finish spot

        if (pieceOwner == PieceOwner.Player)
        {
            playerPiece = true;
            OnPieceFinish?.Invoke(this.gameObject, playerPiece);
        }            
        else if (pieceOwner == PieceOwner.AI)
        {
            playerPiece = false;
            OnPieceFinish?.Invoke(this.gameObject, playerPiece);
        }
            
    }

    private void PieceMoveConfirmed(string phase)
    {
        PhaseManager.OnExitPieceMove -= PieceMoveConfirmed;

        if(pieceState != PieceState.Finished)
            pieceState = PieceState.Waiting;

        OnPieceStateCheck?.Invoke(this.gameObject, pieceState.ToString());  //PieceState ANNOUNCER
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
            BoardCheckRaycast();
        }

        switch (pieceState)
        {
            case PieceState.Waiting:
                if (grabControl.interactionLayerMask != interactableOff)
                {
                    grabControl.interactionLayerMask = interactableOff;
                }
                break;

            case PieceState.Ready:
                if(pieceOwner == PieceOwner.Player)
                {
                    if (grabControl.interactionLayerMask != interactableOn)
                    {
                        grabControl.interactionLayerMask = interactableOn;
                    }
                }
                break;

            case PieceState.OnHand:                
                BoardCheckRaycast();                
                break;

            case PieceState.Dropped:
                if (grabControl.interactionLayerMask != interactableOff)
                {
                    grabControl.interactionLayerMask = interactableOff;
                }
                break;

            case PieceState.AIMoving:
                BoardCheckRaycast();
                break;

            case PieceState.Finished:
                if (onFinishSpot)
                {
                    if (grabControl.interactionLayerMask != interactableOff)
                    {
                        grabControl.interactionLayerMask = interactableOff;
                    }
                }
                break;
        }
        
    }

        //Waiting,  //Pieces not interactable
        //Ready,    //Pieces ready to interact
        //Grabable, //Pieces can be grab
        //OnHand,   //A piece is currently grabbed
        //Dropped,  //Piece is dropped on board
        //Finish    //Piece landed on the finish spot (exit board)

}
