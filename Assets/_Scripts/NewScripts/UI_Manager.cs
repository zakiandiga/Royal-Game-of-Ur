using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public GameObject swallow, stormbird, raven, rooster, eagle;

    public Text turn;
    public Text phase;
    public Text dice;

    public Text pieceStateText;

    //public Text swallowStateText, stormbirdStateText, ravenStateText, roosterStateText, eagleStateText;

    public Text debug;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        PhaseManager.OnPhaseChange += PhaseUpdate;
        PhaseManager.OnExitDiceRoll += NewDiceResult;

        PieceBehaviour.OnPieceStateCheck += PieceStateUpdate;
    }

    private void PhaseUpdate(string phaseChange)
    {
        string currentPhase = phaseChange.ToString();
        phase.text = "Phase: " + currentPhase;
    }

    private void NewDiceResult(int result)
    {
        string resultUI = result.ToString();
        dice.text = "Dice Result: " + resultUI;
    }

    private void PieceStateUpdate(GameObject piece, string pieceState)
    {
        string currentPieceState = pieceState;
        pieceStateText.text = "Piece State: " + piece.name + " " + pieceState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
