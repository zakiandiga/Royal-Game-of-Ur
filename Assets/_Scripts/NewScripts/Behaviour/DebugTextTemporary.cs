using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextTemporary : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        text.text = "debug text ready";
    }

    private void OnEnable()
    {
        PieceBehaviour.OnDebugText += UpdateText;
        BoardManager.OnDebugText += UpdateText;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnDebugText -= UpdateText;
    }

    // Update is called once per frame
    private void UpdateText(string debugText)
    {
        text.text = debugText;
    }
}
