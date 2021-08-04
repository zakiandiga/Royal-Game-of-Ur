using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    [SerializeField] private Animator crossFade;
    [SerializeField] private float crossFadeTime = 0.5f;

    private void Start()
    {
        crossFade.SetTrigger("FadeIn");
    }

    private void OnEnable()
    {
        PhaseManager.OnGameEnd += LoadEnd;
    }

    private void OnDisable()
    {
        PhaseManager.OnGameEnd -= LoadEnd;
    }

    public void StartGame()
    {
        LoadMain();        
    }

    private void LoadMain()
    {
        StartCoroutine(TransitionToMain());        
    }

    private IEnumerator TransitionToMain()
    {
        crossFade.SetTrigger("FadeOut");

        yield return new WaitForSeconds(crossFadeTime);
        SceneManager.LoadScene("main");
    }

    private void LoadEnd(bool playerWin)
    {
        StartCoroutine(TransitionToEnd(playerWin));
    }

    private IEnumerator TransitionToEnd(bool playerWin)
    {
        crossFade.SetTrigger("FadeOut");

        yield return new WaitForSeconds(crossFadeTime);

        if(playerWin)
        {
            //Load player win screen
        }
        else if(!playerWin)
        {
            //Load player lose screen
        }
    }

}
