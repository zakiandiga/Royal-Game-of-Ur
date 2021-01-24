using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Text turn;
    public Text phase;
    public Text dice;

    public Text debug;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        PhaseManager.OnPhaseChange += PhaseUpdate;
        PhaseManager.OnExitDiceRoll += NewDiceResult;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
