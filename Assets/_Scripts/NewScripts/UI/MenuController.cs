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
}
