using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameResultNotification : MonoBehaviour
{
    private Text notifText;

    // Start is called before the first frame update
    void Start()
    {
        notifText = GetComponent<Text>();
        notifText.enabled = false;
    }

    private void OnEnable()
    {
        PhaseManager.OnGameEnd += GameEnding;
    }

    private void OnDisable()
    {
        PhaseManager.OnGameEnd -= GameEnding;
    }

    private void GameEnding(bool playerWin)
    {
        if(playerWin)
        {
            notifText.text = "YOU WIN";
            notifText.enabled = true;
            StartCoroutine(BackToMenuDelay());
        }
        else if(!playerWin)
        {
            notifText.text = "YOU LOSE";
            notifText.enabled = true;
            StartCoroutine(BackToMenuDelay());
        }
    }

    private IEnumerator BackToMenuDelay()
    {
        float delay = 5f;
        yield return new WaitForSeconds(delay);

        BackToMenu();
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
