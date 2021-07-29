using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceResultDisplay : MonoBehaviour
{
    private Text diceResult;
    public enum DisplayOwner
    {
        Player,
        AI
    }
    [SerializeField] private DisplayOwner displayOwner;

    // Start is called before the first frame update
    void Start()
    {
        diceResult = GetComponent<Text>();
        diceResult.text = "";
    }

    private void OnEnable()
    {
        if(displayOwner == DisplayOwner.Player)
        {
            PhaseManager.OnExitDiceRoll += DiceNumberUpdate;
            PhaseManager.OnExitPieceMove += ClearDiceResult;
        }

        else if(displayOwner == DisplayOwner.AI)
        {
            AIAnimationStateMachine.OnAIDiceResultChecked += DiceNumberUpdate;
            AIAnimationStateMachine.OnAIDiceResultResetted += ClearDiceResult;
        }
    }

    private void OnDisable()
    {
        if(displayOwner == DisplayOwner.Player)
        {
            PhaseManager.OnExitDiceRoll -= DiceNumberUpdate;
            PhaseManager.OnExitPieceMove -= ClearDiceResult;
        }

        else if (displayOwner == DisplayOwner.AI)
        {
            AIAnimationStateMachine.OnAIDiceResultChecked -= DiceNumberUpdate;
            AIAnimationStateMachine.OnAIDiceResultResetted -= ClearDiceResult;
        }
    }


    private void DiceNumberUpdate(int result)
    {
        diceResult.text = result.ToString();
    }

    private void ClearDiceResult(string s)
    {
        diceResult.text = "";
    }

}
