using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class PieceBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform startSpawner;
    [SerializeField] private Transform goalSpawner;
    private Vector3 currentPosition;
    private float fallTreshold = 0.5f;
    private bool legalLand = true;

    private Vector3 targetPosition; //vector3 of piece's landing snap position
    private Quaternion defaultRotation;
    [SerializeField] private LayerMask pieceHitMask;

    //might not need dice result
    private int diceNumberResult;
    private int diceBoolResult;

    [SerializeField] private InputActionReference raycasting; //temp

    public static event Action<string> OnRaycastHit;
    

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
        defaultRotation = transform.rotation;
        
    }

    private void OnEnable()
    {
        HandPresence.OnEnterPinch += PieceGrab;
        HandPresence.OnExitPinch += PieceDrop;
        PhaseManager.OnExitDiceRoll += DiceResultCheck;

        raycasting.action.Enable(); //temp

    }

    private void OnDisable()
    {
        HandPresence.OnEnterPinch -= PieceGrab;
        HandPresence.OnExitPinch -= PieceDrop;
        PhaseManager.OnExitDiceRoll -= DiceResultCheck;

        raycasting.action.Disable(); //temp
    }

    private void DiceResultCheck(int result)
    {
        diceNumberResult = result;

        //Calculate this piece movement
    }
       
    private bool CheckLegalMove() //should be on PhaseManager or PlayerManager?
    {
        //check all legal move possibility
        return true; //temporary

    }

    public void GrabColliderEnter()
    {
        if (pieceState == PieceState.Ready)
        {
            pieceState = PieceState.Grabable;
            //Debug.Log(this.gameObject.name + " is GRABBABLE");
        }
    }

    public void GrabColliderExit()
    {
        if (pieceState == PieceState.Grabable)
        {
            pieceState = PieceState.Ready;
            //Debug.Log(this.gameObject.name + " is NOT GRABBABLE");
        }
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
        //transform.rotation =  //rotate to default rotation

        if(pieceState == PieceState.OnHand)
        {
            //if move is legal, drop on the selected square, then MovePiece(Vector3 selectedSquare.position)

            //if dropped outside altar, drop on startSpawner (to be handled as dropped not on board)

            //if move is not legal or if dropped not on board, drop on current position
            

            pieceState = PieceState.Dropped;
            currentPosition = transform.position;
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

    private void RaycastingTest()
    {
        float range = 500f;
        string targetHit = null;
        int targetHitConvert;

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(transform.position, Vector3.down, Color.red, 1, true);
        if (Physics.Raycast(ray, out hit, range, pieceHitMask))
        {

            if (targetHit == null || targetHit != hit.transform.name)
            {
                targetHit = hit.transform.name;
                int.TryParse(targetHit, out targetHitConvert);

                targetPosition = hit.transform.position;
                OnRaycastHit?.Invoke(hit.transform.name);
            }

        }
    }

    public void OnHandEnter()
    {
        if(pieceState != PieceState.OnHand)
        {
            pieceState = PieceState.OnHand;
        }
    }

    public void OnHandExit()
    {
        if(pieceState != PieceState.Ready)
        {
            pieceState = PieceState.Ready;
        }

        //Landing at square handler
        float lerpTime = 0.5f;
        Vector3 releasePosition = transform.position;


        transform.rotation = defaultRotation;

        transform.position = targetPosition; //Vector3.Lerp(releasePosition, targetPosition, lerpTime * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if(raycasting.action.triggered)
        {
            RaycastingTest();
        }

        if(diceBoolResult > 0 && diceNumberResult > 0 && pieceState == PieceState.Waiting) //need to consider if the player have valid move
        {
            if(CheckLegalMove())
            {
                pieceState = PieceState.Ready;
            }            
 
        }

        if(pieceState == PieceState.OnHand)
        {
            RaycastingTest();
        }



    }
}
