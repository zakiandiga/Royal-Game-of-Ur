using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject mainBoard;

    public List<GameObject> playerSquare; //Square that response to player's interaction
    public List<GameObject> aISquare; //Square that response to AI's interaction
    public List<PieceBehaviour> playerPieces;
    //playerPieces guide
    //playerPieces[0] = swallow
    //playerPieces[1] = stormbird
    //playerPieces[2] = raven
    //playerPieces[3] = rooster
    //playerPieces[4] = eagle    

    [SerializeField] private int diceResult;
    //[SerializeField] private int currentSquareNumber = 0;
    //[SerializeField] private int legalMove;
    private int finishSquareIndex = 15;

    [SerializeField] private LayerMask hitMask;

    public static event Action<bool, int> OnOccupiedSpace;
    public static event Action<int> OnLegalMoveAvailable;
    public static event Action<bool, bool, bool> OnPieceDropHandlerDone;
    public static event Action<string> OnDebugText;


    private void OnEnable()
    {
        PieceBehaviour.OnHoveringPieces += MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider += ExitPieceCollider;
        PieceBehaviour.OnRaycastHit += SquareHitHandler;
        PieceBehaviour.OnPieceDropped += PieceDropHandler;
        PhaseManager.OnExitDiceRoll += DiceRollObserver;
        PhaseManager.OnPlayerDelayCheck += SquareTenantCheck;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnHoveringPieces -= MoveValidHighlight;
        PieceBehaviour.OnExitPieceCollider -= ExitPieceCollider;
        PieceBehaviour.OnRaycastHit -= SquareHitHandler;
        PieceBehaviour.OnPieceDropped -= PieceDropHandler;
        PhaseManager.OnExitDiceRoll -= DiceRollObserver;
        PhaseManager.OnPlayerDelayCheck -= SquareTenantCheck;
    }

    private void DiceRollObserver(int dice)
    {
        diceResult = dice;
        CheckPlayerPieceLegalMove();
    }

    public void RollDiceDebug()
    {
        diceResult = UnityEngine.Random.Range(1, 10);
        Debug.Log("Debug dice result = " + diceResult);
        CheckPlayerPieceLegalMove();
    }

    private void CheckPlayerPieceLegalMove()
    {
        Debug.Log("Checking pieces legal move");

        int legalMoveAmount = 0;
        for (int i = 0; i < playerPieces.Count; ++i)
        {
            int targetSquareIndex = playerPieces[i].currentSquare + diceResult;

            if (targetSquareIndex > finishSquareIndex) //if the targetsquare is beyond the finish line
            {
                Debug.Log(playerPieces[i].name + "cannot move beyond finish line");
                playerPieces[i].hasValidMove = false;
            }

            if (targetSquareIndex <= finishSquareIndex && playerPieces[i].hasLaunched) //if the piece is on board (is launched)
            {
                

                //Check if the target square doesn't have a player piece
                if (playerSquare[targetSquareIndex].GetComponent<SquareBehaviour>().squareTenant != SquareBehaviour.SquareTenant.White)
                {
                    legalMoveAmount += 1;
                    playerPieces[i].playerTargetSquare = targetSquareIndex;
                    playerPieces[i].hasValidMove = true;
                }

                if (playerSquare[targetSquareIndex].GetComponent<SquareBehaviour>().squareTenant == SquareBehaviour.SquareTenant.White)
                {
                    playerPieces[i].hasValidMove = false;
                }
            }

            else if (targetSquareIndex <= finishSquareIndex && !playerPieces[i].hasLaunched) //if the piece is no launched
            {
                int currentPiece = i;

                //rule of launching piece to the board
                switch (currentPiece) 
                {
                    case 0:
                        if (targetSquareIndex == 2 || targetSquareIndex == 4)
                        {
                            legalMoveAmount += 1;
                            playerPieces[i].playerTargetSquare = targetSquareIndex;
                            playerPieces[i].hasValidMove = true;
                            playerPieces[i].hasLaunched = true;
                            Debug.Log("Player Swallow can move");
                        }
                        else
                        {
                            playerPieces[i].hasValidMove = false;
                        }
                        break;
                    case 1:
                        if (targetSquareIndex == 5)
                        {
                            legalMoveAmount += 1;
                            playerPieces[i].playerTargetSquare = targetSquareIndex;
                            playerPieces[i].hasValidMove = true;
                            playerPieces[i].hasLaunched = true;
                            Debug.Log("Player Stormbird can move");
                        }
                        else
                        {
                            playerPieces[i].hasValidMove = false;
                        }
                        break;
                    case 2:
                        if (targetSquareIndex == 6)
                        {
                            legalMoveAmount += 1;
                            playerPieces[i].playerTargetSquare = targetSquareIndex;
                            playerPieces[i].hasValidMove = true;
                            playerPieces[i].hasLaunched = true;
                            Debug.Log("Player Raven can move");
                        }
                        else
                        {
                            playerPieces[i].hasValidMove = false;
                        }
                        break;
                    case 3:
                        if (targetSquareIndex == 7)
                        {
                            legalMoveAmount += 1;
                            playerPieces[i].playerTargetSquare = targetSquareIndex;
                            playerPieces[i].hasValidMove = true;
                            playerPieces[i].hasLaunched = true;
                            Debug.Log("Player Rooster can move");
                        }
                        else
                        {
                            playerPieces[i].hasValidMove = false;
                        }
                        break;
                    case 4:
                        if (targetSquareIndex == 10)
                        {
                            legalMoveAmount += 1;
                            playerPieces[i].playerTargetSquare = targetSquareIndex;
                            playerPieces[i].hasValidMove = true;
                            playerPieces[i].hasLaunched = true;
                            Debug.Log("Player Eagle can move");
                        }
                        else
                        {
                            playerPieces[i].hasValidMove = false;
                        }
                        break;

                }
            }

        }

        if (legalMoveAmount <= 0)
        {
            //tell piece behaviour that there is no legal move, skip state to PieceState.Dropped
            OnLegalMoveAvailable?.Invoke(legalMoveAmount);
            //tell UI to display 'No legal move available' info
            OnDebugText?.Invoke("No legal move available");
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
                highlight.material.color = Color.green;
                highlight.enabled = true;
            }

            if(!isLegalSquare)
            {
                playerSquare[i].GetComponent<MeshRenderer>().enabled = false;
            }
        
        }
    }

    private void PieceDropHandler(bool legalDrop, int targetSquare, bool isPlayerPiece)
    {
        if(isPlayerPiece)
        {
            PlayerPieceDrop(legalDrop, targetSquare);
        }

        else if(!isPlayerPiece)
        {
            AIPieceDrop(legalDrop, targetSquare);
        }
    }

    //turn the square highlight off after the piece is dropped
    private void PlayerPieceDrop(bool legalDrop, int targetSquare) 
    {
        var squareTenant = playerSquare[targetSquare].GetComponent<SquareBehaviour>().squareTenant;
        bool isRosette = playerSquare[targetSquare].GetComponent<SquareBehaviour>().isRosette;
        bool isFinish = playerSquare[targetSquare].GetComponent<SquareBehaviour>().isFinish;
        bool isKicking;

        if(legalDrop)
        {
            if(squareTenant == SquareBehaviour.SquareTenant.Black)
            {
                isKicking = true;
                OnPieceDropHandlerDone?.Invoke(isRosette, isKicking, isFinish);
            }
            else if(squareTenant == SquareBehaviour.SquareTenant.Empty)
            {
                //drop casually
                isKicking = false;
                OnPieceDropHandlerDone?.Invoke(isRosette, isKicking, isFinish);
            }
        }

        foreach (GameObject square in playerSquare)
        {
            MeshRenderer highlight = square.GetComponent<MeshRenderer>();
            if(highlight.enabled == true)
            {
                highlight.enabled = false;
            }
        }
    }

    private void AIPieceDrop(bool legalDrop, int targetSquare)
    {
        //AI drop doesn't need to consider legalDrop value because it is always true at this point
        var squareTenant = aISquare[targetSquare].GetComponent<SquareBehaviour>().squareTenant;
        bool isRosette = aISquare[targetSquare].GetComponent<SquareBehaviour>().isRosette;
        bool isFinish = aISquare[targetSquare].GetComponent<SquareBehaviour>().isFinish;
        bool isKicking;

        Debug.Log("targetSquare: " + targetSquare);
        Debug.Log("squareTenant: " + squareTenant);
        Debug.Log("isRosette: " + isRosette);
        Debug.Log("isFinish: " + isFinish);
        
        if (squareTenant == SquareBehaviour.SquareTenant.White)
        {
            isKicking = true;
            Debug.Log("BoardManager: AI isKicking = " + isKicking);
            OnPieceDropHandlerDone?.Invoke(isRosette, isKicking, isFinish);
            //this event is being subscribed by the piece during DropPiece(), 
            //and is being unsubscribe during FinalizePieceDrop()
        }
        else if (squareTenant == SquareBehaviour.SquareTenant.Empty)
        {
            isKicking = false;
            Debug.Log("BoardManager: AI isKicking = " + isKicking);
            OnPieceDropHandlerDone?.Invoke(isRosette, isKicking, isFinish);
        }
    }

    public void SquareTenantCheck(string source)
    {
        for (int i = 0; i < playerSquare.Count; ++i)
        {
            playerSquare[i].GetComponent<SquareBehaviour>().CheckTenant();
        }
    }
}
