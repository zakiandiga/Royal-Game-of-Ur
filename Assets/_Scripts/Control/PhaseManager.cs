using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    #region GameState
    [SerializeField] private PlayerState playerState;
    public enum PlayerState
    {
        Delay,
        Waiting,
        DiceRoll,
        PieceMove,
    }

    [SerializeField] private WorldState worldState;
    public enum WorldState
    {
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
    public static event Action<PhaseManager> OnEnterDiceRoll;
    public static event Action<PhaseManager> OnExitDiceRoll;
    public static event Action<PhaseManager> OnEnterPieceMove;
    public static event Action<PhaseManager> OnExitPieceMove;
    #endregion

    #region Start/OnDestroy
    void Start()
    {
        DiceBehaviour.OnDiceNumberResult += DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult += DiceBoolResultCheck;
    }

    private void OnDestroy()
    {
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
    }
    #endregion

    #region DiceResultObserver
    private void DiceNumberResultCheck(int result)
    {
        Debug.Log("Recieve dice number result from DiceBehaviour, value: " + result);
        //Display on the UI?
        numberDiceThrown = true;

    }

    private void DiceBoolResultCheck(int result)
    {
        Debug.Log("Recieve dice bool result from DiceBehaviour, value: " + result);
        //Display on the UI?
        boolDiceThrown = true;

    }
    #endregion

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
                    OnEnterDiceRoll?.Invoke(this);
                }
                break;

            case PlayerState.DiceRoll:
                if(numberDiceThrown && boolDiceThrown)
                {
                    playerState = PlayerState.PieceMove;
                    OnExitDiceRoll?.Invoke(this);
                    OnEnterPieceMove?.Invoke(this);
                }
                break;

            case PlayerState.PieceMove:
                if(pieceMoved)
                {
                    playerState = PlayerState.Waiting;
                    OnExitPieceMove?.Invoke(this);
                }
                break;

            case PlayerState.Waiting:
                //wait until AI/Other player finish their turn
                if(worldState == WorldState.playerTurn)
                {
                    playerState = PlayerState.Delay;
                }
                break;

        }

    }
}
