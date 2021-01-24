using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject mainBoard;

    public List<GameObject> playerSquare; //Square that response to player's interaction
    public List<GameObject> aISquare; //Square that response to AI's interaction

    [SerializeField] private int diceResult = 2;
    [SerializeField] private int currentSquareNumber = 0;
    [SerializeField] private int legalMove;

    private void Start()
    {
        legalMove = diceResult + currentSquareNumber; //test
    }

    private void OnEnable()
    {
        PieceBehaviour.OnMoveValidCheck += MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider += ExitPieceCollider;
        PieceBehaviour.OnRaycastHit += SquareHitHandler;
        PhaseManager.OnExitDiceRoll += LegalCounter;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnMoveValidCheck -= MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider -= ExitPieceCollider;
        PieceBehaviour.OnRaycastHit -= SquareHitHandler;
        PhaseManager.OnExitDiceRoll -= LegalCounter;
    }

    private void LegalCounter(int dice)
    {
        diceResult = dice;
    }

    private void MoveValidHighlight (int squareIndex)
    {
        Debug.Log("legal move square is: Square " + squareIndex);

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
}
