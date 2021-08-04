using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceMoveSkipNotif : MonoBehaviour
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
        PhaseManager.OnPlayerSkipPiece += DisplayNotification;
        AIAnimationStateMachine.AI_TurnFinished += DisplayNotification;
    }

    private void OnDisable()
    {
        PhaseManager.OnPlayerSkipPiece -= DisplayNotification;
    }

    private void DisplayNotification (string msg)
    {
        if (msg == "player skip")
        {
            notifText.text = "You have no available move";
            notifText.enabled = true;
            StartCoroutine(DisplayNotificationDelay());
        }
        else if (msg == "AI skip")
        {
            notifText.text = "The opponent has no available move";
            notifText.enabled = true;
            StartCoroutine(DisplayNotificationDelay());
        }
        //add another string condition here if needed

        else
        {
            if(notifText.enabled == true)
                notifText.enabled = false;
        }

    }

    private IEnumerator DisplayNotificationDelay()
    {        
        float delay = 2.5f;        

        yield return new WaitForSeconds(delay);

        notifText.enabled = false;
    }

}
