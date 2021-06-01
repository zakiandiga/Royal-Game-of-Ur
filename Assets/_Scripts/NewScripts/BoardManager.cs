using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject mainBoard;

    public List<GameObject> playerSquare; //Square that response to player's interaction
    public List<GameObject> aISquare; //Square that response to AI's interaction
    public List<PieceBehaviour> whitePieces;

    [SerializeField] private int diceResult = 2;
    [SerializeField] private int currentSquareNumber = 0;
    [SerializeField] private int legalMove;

    [SerializeField] private LayerMask hitMask;

    public static event Action<bool, int> OnOccupiedSpace;
    public static event Action<int> OnLegalMoveAvailable;
    public static event Action<string> OnDebugText;


    private void Start()
    {
        legalMove = diceResult + currentSquareNumber; //test

    }

    private void OnEnable()
    {
        PieceBehaviour.OnHoveringPieces += MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider += ExitPieceCollider;
        PieceBehaviour.OnRaycastHit += SquareHitHandler;
        PieceBehaviour.OnPieceDropped += PieceDropHandler;
        PhaseManager.OnExitDiceRoll += LegalCounter;
        PhaseManager.OnPlayerDelayCheck += SquareDataCheck;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnHoveringPieces -= MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider -= ExitPieceCollider;
        PieceBehaviour.OnRaycastHit -= SquareHitHandler;
        PieceBehaviour.OnPieceDropped -= PieceDropHandler;
        PhaseManager.OnExitDiceRoll -= LegalCounter;
        PhaseManager.OnPlayerDelayCheck -= SquareDataCheck;
    }

    private void LegalCounter(int dice)
    {
        diceResult = dice;
        CheckPieceLegalMove();
    }

    public void RollDiceDebug()
    {
        diceResult = UnityEngine.Random.Range(1, 10);
        Debug.Log("Debug dice result = " + diceResult);
        CheckPieceLegalMove();
    }

    private void CheckPieceLegalMove()
    {
        Debug.Log("Checking pieces legal move");

        int legalMoveAmount = 0;
        for (int i = 0; i < whitePieces.Count; ++i)
        {
            int finishSquareIndex = 15; //Temporary 'piece exit' handler

            int currentSquare = whitePieces[i].currentSquare;

            Debug.Log("currentSquare = " + currentSquare);

            int targetSquareIndex = currentSquare + diceResult;           

            if(targetSquareIndex > finishSquareIndex) //Temporary 'piece exit' handler
            {
                targetSquareIndex = finishSquareIndex;
            }

            //Check if the target square doesn't have a player piece
            if (playerSquare[targetSquareIndex].GetComponent<SquareBehaviour>().squareTenant != SquareBehaviour.SquareTenant.Player)
            {
                legalMoveAmount += 1;
                whitePieces[i].targetSquare = targetSquareIndex;
                whitePieces[i].hasValidMove = true;
            }

            if (playerSquare[targetSquareIndex].GetComponent<SquareBehaviour>().squareTenant == SquareBehaviour.SquareTenant.Player)
            {
                whitePieces[i].hasValidMove = false;
            }

        }

        if (legalMoveAmount <= 0)
        {
            //tell piece behaviour that there is no legal move, skip state to PieceState.Dropped
        }

        if(legalMoveAmount > 0)
        {
            //tell piece behaviour that there is legal move, proceed state to PieceState.Ready
            OnLegalMoveAvailable?.Invoke(legalMoveAmount);
            Debug.Log("legal move amount = " + legalMoveAmount.ToString());
            OnDebugText?.Invoke("Legal move = " + legalMoveAmount.ToString());

        }
    }

    private void MoveValidHighlight (int index)
    {
        int squareIndex = index;

        MeshRenderer highlight = playerSquare[squareIndex].GetComponent<MeshRenderer>();
        highlight.enabled = true;
        highlight.material.color = Color.green;


    }

    private void ExitPieceCollider (PieceBehaviour piece)
    {
        foreach (GameObject square in playerSquare)
        {
            MeshRenderer highlight = square.GetComponent<MeshRenderer>();
            if (highlight.enabled == true)
            {
                highlight.enabled = false;
            }

        }
    }

    //turn on the square highlight on when the piece hover on top of the board 
    //if the move is legal
    private void SquareHitHandler(string squareName, bool isLegal)     
    {
        bool isLegalSquare = isLegal;        
        
        for (int i = 0; i < playerSquare.Count; i++)
        {
            if(playerSquare[i].gameObject.name == squareName && isLegalSquare)
            {
                MeshRenderer highlight = playerSquare[i].GetComponent<MeshRenderer>();
                highlight.enabled = true;
                highlight.material.color = Color.green;
            }

            if(!isLegalSquare)
            {
                playerSquare[i].GetComponent<MeshRenderer>().enabled = false;
            }
        
        }
    }

    //turn the square highlight off after the piece is dropped
    private void PieceDropHandler(bool legalDrop) 
    {
        foreach (GameObject square in playerSquare)
        {
            MeshRenderer highlight = square.GetComponent<MeshRenderer>();
            if(highlight.enabled == true)
            {
                highlight.enabled = false;
            }
        }
    }

    public void SquareDataCheck(string source)
    {
        for (int i = 0; i < playerSquare.Count; ++i)
        {
            playerSquare[i].GetComponent<SquareBehaviour>().CheckData();
        }
    }
}
