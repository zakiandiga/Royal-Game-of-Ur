using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class AI_Controller : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    [SerializeField] private Transform targetDestination;

    bool isWinRoll;


    private EnemyState enemyState = EnemyState.Walkin;
    public enum EnemyState
    {
        Walkin,
        Starting,
        RollDice,
        MovePiece,
        Waiting
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        StartBehaviour(targetDestination.position);

        anim.SetBool("isIdle", false);
        anim.SetBool("isWalking", true);
    }

    private bool OnRollResult(bool win) //observer to first turn roll
    {
        if (win)
            return true;
        else
            return false;
    }

    private void StartBehaviour(Vector3 position)
    {
        anim.SetBool("isWalking", true);
        agent.SetDestination(position);   

    }


    // Update is called once per frame
    void Update()
    {


        switch (enemyState)
        {
            case EnemyState.Walkin:
                if(Vector3.Distance(targetDestination.position, transform.position) < 0.5f)
                {                    
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isIdle", true);
                    enemyState = EnemyState.Starting;
                    Debug.Log(enemyState);
                }
                break;
            case EnemyState.Starting:
                //Starting behaviour
                //Waiting player starting roll
                //Do starting roll
                //wait for the result, if win, exit to roll dice, if !win, exit to waiting
                break;
            case EnemyState.Waiting:
                //set observer
                //On AI turn, exit to roll dice
                break;
            case EnemyState.RollDice:
                //Run RollDice()
                //Observe roll dice result
                //OnRollDiceResult(int), exit to MovePiece
                break;
            case EnemyState.MovePiece:
                //run if false, CalculatePossibility()
                //On calculate, move piece
                //exit to waiting
                break;


                
        }
    }
}
