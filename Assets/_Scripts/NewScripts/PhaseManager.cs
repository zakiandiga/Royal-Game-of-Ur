using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhaseManager : MonoBehaviour
{
    private int totalDiceResult, numberDiceResult, boolDiceResult;

    [SerializeField] private InputActionReference switchPlayer; //TEMP

    #region GameState
    [SerializeField] private PlayerState playerState = PlayerState.Delay; //Temporary for Dice Debug
    public enum PlayerState
    {
        Delay, //Cleanup value before turn start
        Waiting, //Waiting during AI turn, can't interact with object
        DiceRoll, //Player can roll both dices
        PieceMove, //Player can move piece (if theres any possible move), update score if any
    }

    [SerializeField] private WorldState worldState;
    public enum WorldState
    {
        startingGame, //Preparation to start the game (Decide who's first)
        playerTurn,
        aiTurn,
        playerWin,
        aiWin
    }

    private bool numberDiceThrown = false;
    private bool boolDiceThrown = false;
    private bool pieceMoved = false;
    #endregion

    #region EventAnnouncer
    public static event Action<string> OnPhaseChange;
    public static event Action<string> OnPlayerDelayCheck;
    public static event Action<PhaseManager> OnEnterDiceRoll;
    public static event Action<int> OnExitDiceRoll;
    public static event Action<PhaseManager> OnEnterPieceMove;
    public static event Action<PhaseManager> OnExitPieceMove;
    public static event Action<PhaseManager> OnAITurnStart;

    public static event Action<string> OnDebugText;
    #endregion

    private List<GameObject> playerFinishedPieces;
    private int maxPlayerFinishedPiece = 5;
    private int currentPlayerFinishedPiece = 0;

    private int maxAIFinishedPiece = 5;
    private int currentAIFinishedPiece = 0;

    #region Start/OnDestroy
    void Start()
    {
        //Testing
        worldState = WorldState.playerTurn;
    }

    private void OnEnable()
    {
        DiceBehaviour.OnDiceNumberResult += DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult += DiceBoolResultCheck;
        PieceBehaviour.OnPieceDropFinalized += PieceDropCheck;
        PieceBehaviour.OnPieceFinish += PieceFinishCheck;
        BoardManager.OnLegalMoveAvailable += PlayerSkipPieceMove;
        AIAnimationStateMachine.AI_TurnFinished += SwitchToPlayerTurn;

        switchPlayer.action.Enable(); //TEMP
    }


    private void OnDisable()
    {
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
        PieceBehaviour.OnPieceDropFinalized -= PieceDropCheck;
        PieceBehaviour.OnPieceFinish -= PieceFinishCheck;
        BoardManager.OnLegalMoveAvailable -= PlayerSkipPieceMove;
        AIAnimationStateMachine.AI_TurnFinished -= SwitchToPlayerTurn;

        switchPlayer.action.Disable(); //TEMP
    }
    #endregion

    #region PlayerDiceResultObserver
    private void DiceNumberResultCheck(int numResult, bool aIDice)
    {
        if(!aIDice)
        {
            Debug.Log("Recieve dice number result from DiceBehaviour, value: " + numResult);
            numberDiceResult = numResult;
            //Display on the UI?
            numberDiceThrown = true;
        }

    }

    private void DiceBoolResultCheck(int boolResult, bool aIDice)
    {
        if(!aIDice)
        {
            Debug.Log("Recieve dice bool result from DiceBehaviour, value: " + boolResult);
            boolDiceResult = boolResult;
            //Display on the UI?
            boolDiceThrown = true;
        }
    }

    private void TotalDiceResultCheck()
    {
        if(boolDiceResult == 0)
        {
            totalDiceResult = numberDiceResult;
        }
        if(boolDiceResult == 1)
        {
            if (numberDiceResult >= 1 && numberDiceResult <= 3)
                totalDiceResult = numberDiceResult + 4;
            else if (numberDiceResult == 4)
                totalDiceResult = 10;
        }
        Debug.Log("Total Dice result = " + totalDiceResult);
    }
    #endregion

    private void PlayerSkipPieceMove(int playerLegalMoveAmount)
    {
        if(playerLegalMoveAmount <= 0)
        {
            playerState = PlayerState.Waiting;
            worldState = WorldState.aiTurn;

            OnExitPieceMove?.Invoke(this);
            OnPhaseChange?.Invoke(playerState.ToString()); //for UI
        }
    }

    private void PieceDropCheck(bool legalDrop, bool isRosette, bool isPlayerPiece)
    {
        if(legalDrop)
        {
            if(isPlayerPiece)
            {
                if (!isRosette)
                {
                    playerState = PlayerState.Waiting;
                    worldState = WorldState.aiTurn;

                    OnExitPieceMove?.Invoke(this);
                    OnPhaseChange?.Invoke(playerState.ToString()); //for UI
                }

                else if (isRosette)
                {
                    playerState = PlayerState.Delay;

                    OnExitPieceMove?.Invoke(this);
                    OnPhaseChange?.Invoke(playerState.ToString()); //for UI
                }
            }

        }
        //pieceMoved = legalDrop;
    }

    private void PieceFinishCheck(GameObject piece, bool playerPiece)
    {
        OnDebugText?.Invoke(piece.gameObject.name + " just land on the finish square");
        //playerFinishedPieces.Add(piece.gameObject);

        if(playerPiece)
        {
            currentPlayerFinishedPiece += 1;

            if (currentPlayerFinishedPiece >= maxPlayerFinishedPiece)
            {
                Debug.Log("PLAYER WIN");
                OnDebugText?.Invoke("Player WIN!!");
                worldState = WorldState.playerWin;

                //Win event
            }

            else if (currentPlayerFinishedPiece < maxPlayerFinishedPiece)
            {
                playerState = PlayerState.Waiting;
                worldState = WorldState.aiTurn;

                OnExitPieceMove?.Invoke(this);
                OnPhaseChange?.Invoke(playerState.ToString());
            }
        }

        if(!playerPiece) //if AI piece's land on finish
        {
            currentAIFinishedPiece += 1;

            if(currentAIFinishedPiece >= maxAIFinishedPiece)
            {
                Debug.Log("AI WIN");
                OnDebugText?.Invoke("Player LOSE!!");
                worldState = WorldState.aiWin;

                //Lose event
            }
            
            else if (currentAIFinishedPiece < maxAIFinishedPiece)
            {

            }
        }

    }

    private void PieceMoveCheck() //On piece moved
    {
        pieceMoved = true;
    }

    public void SwitchToPlayerTurn(string condition) //set to private when Skip AI Turn debug button isn't used anymore
    {
        worldState = WorldState.playerTurn;
    }

    // Update is called once per frame
    void Update()
    {
        if(switchPlayer.action.triggered) //Debug force to player turn
        {
            SwitchToPlayerTurn("DebugButton");
        }

        switch (playerState)
        {
            case PlayerState.Delay:
                //clean up requirement checklist
                totalDiceResult = 0;
                numberDiceResult = 0;
                boolDiceResult = 0;
                if (numberDiceThrown)
                    numberDiceThrown = false;
                if (boolDiceThrown)
                    boolDiceThrown = false;
                if (pieceMoved)
                    pieceMoved = false;
                OnPlayerDelayCheck?.Invoke("PhaseManager");

                //if all the clean up requirement met, exit to DiceRoll
                if (!numberDiceThrown && !boolDiceThrown && !pieceMoved)
                {
                    playerState = PlayerState.DiceRoll;
                    OnPhaseChange?.Invoke(playerState.ToString());
                    OnEnterDiceRoll?.Invoke(this);
                }
                break;

            case PlayerState.DiceRoll:
                if(numberDiceThrown && boolDiceThrown && totalDiceResult == 0)
                {
                    TotalDiceResultCheck();                    
                }
                if(totalDiceResult > 0)
                {
                    playerState = PlayerState.PieceMove;
                    OnExitDiceRoll?.Invoke(totalDiceResult);
                    OnPhaseChange?.Invoke(playerState.ToString());
                    Debug.Log("Player state now = " + playerState);
                }
                break;

            case PlayerState.PieceMove:
                //handled by PieceDropCheck()

                //Handle if player has no valid move here
                break;

            case PlayerState.Waiting:
                //wait until AI/Other player finish their turn


                if(worldState == WorldState.playerTurn) //Exiting PlayerState.Waiting
                {
                    if(playerState != PlayerState.Delay)
                    {
                        playerState = PlayerState.Delay;
                        OnPhaseChange?.Invoke(playerState.ToString());
                    }

                }
                break;

        }

    }
}
