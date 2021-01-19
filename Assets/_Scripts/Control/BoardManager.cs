using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject mainBoard;

    public List<GameObject> squares;


    [SerializeField] private int diceResult = 2;
    [SerializeField] private int currentSquareNumber = 0;
    [SerializeField] private int legalMove;

    private void Start()
    {
        PieceBehaviour.OnRaycastHit += SquareHitHandler;
        PhaseManager.OnExitDiceRoll += LegalCounter;

        legalMove = diceResult + currentSquareNumber;
        Debug.Log("Legal square to move = " + legalMove);

    }

    private void LegalCounter(int dice)
    {
        diceResult = dice;
    }

    private void SquareHitHandler(string squareName)
    {
        //legalMove = currentSquareNumber + diceResult;
        //int squareHitNumber;
        //int.TryParse(squareName, out squareHitNumber);
        string legalMoveString = legalMove.ToString();
        
        for (int i = 0; i < squares.Count; i++)
        {
            int squareIndex = i + diceResult;
            if(squareIndex > squares.Count)
            {
                squareIndex = 15;
            }

            if(squares[squareIndex].name.ToString() == squareName)
            {
                squares[squareIndex].GetComponent<MeshRenderer>().enabled = true;
                Debug.Log("This square is legal " + squares[squareIndex].name);
            }

            if(squares[squareIndex].name.ToString() != squareName)
            {
                squares[squareIndex].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        /*
        foreach (GameObject square in squares)
        {
            //Debug.Log(legalMove.ToString());
            //int.TryParse(square.name, out currentSquareNumber);

            //int.TryParse(square.name, out squareHitConvert);

            if(squareName == legalMove.ToString() && square.GetComponent<MeshRenderer>().enabled == false)
            {
                square.GetComponent<MeshRenderer>().enabled = true;
                Debug.Log("Target square (" + square.name + ") is LEGAL");
            }

            if(squareName != legalMove.ToString())
            {
                square.GetComponent<MeshRenderer>().enabled = false;
                //Debug.Log("Target ILLEGAL!");
                //Announce illegal

            }
        }
        */

        /*
        foreach(GameObject square in squares)
        {
            int.TryParse(square.name, out squareNumber);
            int legalSquare = squareNumber + diceResult;

            if(square.name == squareName && square.GetComponent<MeshRenderer>().enabled == false)
            {
                square.GetComponent<MeshRenderer>().enabled = true;
                Debug.Log("TARGET = " + square.name);
            }
            
            if(square.name != squareName && square.GetComponent<MeshRenderer>().enabled == true)
            {
                square.GetComponent<MeshRenderer>().enabled = false;
            }
            
        }
        */
    }
}
