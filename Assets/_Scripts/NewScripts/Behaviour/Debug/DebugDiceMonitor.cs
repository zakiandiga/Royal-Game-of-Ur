using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugDiceMonitor : MonoBehaviour
{
    [SerializeField] Text boolText;
    [SerializeField] Text numText;
    [SerializeField] GameObject boolDice, numDice;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        DiceBehaviour.OnDiceStateChange += UpdateText;
    }

    // Update is called once per frame
    void UpdateText(GameObject dice, string textUpdate)
    {
        if(dice == boolDice)
        {
            boolText.text = dice.gameObject.name + " state: " + textUpdate;
        }

        if(dice == numDice)
        {
            numText.text = dice.gameObject.name + " state: " + textUpdate;
        }
    }
}
