using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    private int totalDiceResult, numberDiceResult, boolDiceResult;

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
    public static event Action<PhaseManager> OnEnterDiceRoll;
    public static event Action<int> OnExitDiceRoll;
    public static event Action<PhaseManager> OnEnterPieceMove;
    public static event Action<PhaseManager> OnExitPieceMove;
    #endregion

    #region Start/OnDestroy
    void Start()
    {
        DiceBehaviour.OnDiceNumberResult += DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult += DiceBoolResultCheck;
        PieceBehaviour.OnPieceDropped += PieceDropCheck;
    }

    private void OnDestroy()
    {
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
    }
    #endregion

    #region DiceResultObserver
    private void DiceNumberResultCheck(int numResult)
    {
        Debug.Log("Recieve dice number result from DiceBehaviour, value: " + numResult);
        numberDiceResult = numResult;
        //Display on the UI?
        numberDiceThrown = true;

    }

    private void DiceBoolResultCheck(int boolResult)
    {
        Debug.Log("Recieve dice bool result from DiceBehaviour, value: " + boolResult);
        boolDiceResult = boolResult;
        //Display on the UI?
        boolDiceThrown = true;

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

    private void PieceDropCheck(bool legalDrop)
    {
        pieceMoved = legalDrop;
    }

    private void PieceMoveCheck() //On piece moved
    {
        pieceMoved = true;
    }

    // Update is called once per frame
    void Update()
    {
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
                if(pieceMoved) //if piece moved legally
                {
                    playerState = PlayerState.Waiting;

                    OnExitPieceMove?.Invoke(this);
                    OnPhaseChange?.Invoke(playerState.ToString());
                }
                break;

            case PlayerState.Waiting:
                //wait until AI/Other player finish their turn
                if(worldState == WorldState.playerTurn)
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
