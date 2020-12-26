using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour {

    public GameObject DieRespawn;
    public GameObject[] PieceRespawn;
    public GameObject[] boardspots;

    public GameObject[] pieces;
    public GameObject[] dice;

    public GameObject GameManager;
    public AIAnimationStateMachine aistate;

    public GameObject[] blackdice;
    public GameObject BlackDieRespawn;

    public GameObject gameboard;
    public GameObject[] insidetriggers;
    public InsideTrigger[] its;

    // Use this for initialization
    void Start () {
        aistate = GameManager.GetComponent<AIAnimationStateMachine>();

        for (int i = 0; i < pieces.Length; i++)
        {
            PieceRespawn[i].transform.position = pieces[i].transform.position;
        }

        its = new InsideTrigger[3];
        for (int i = 0; i < its.Length; i++)
        {
            its[i] = insidetriggers[i].GetComponent<InsideTrigger>();
        }
    }
	
	// Update is called once per frame
	void Update () {
        // falls off table
		foreach (GameObject die in dice)
        {
            if (die.transform.position.y < 0.5f)
            {
                die.transform.position = DieRespawn.transform.position;
            }
        }
        //falls off table
        foreach (GameObject piece in pieces)
        {
            if (piece.transform.position.y < 0.5f)
            {
                //respawn piece back on board spot
                if (aistate.board[int.Parse(piece.gameObject.name) - 1] > 0)
                {
                    piece.transform.position = boardspots[aistate.board[int.Parse(piece.gameObject.name) - 1]].transform.position;
                }
                //respawn piece back to table
                else
                {
                    piece.transform.position = PieceRespawn[int.Parse(piece.gameObject.name) - 1].transform.position;
                }
            }
        }
        //falls off table
        foreach (GameObject blackdie in blackdice)
        {
            if (blackdie.transform.position.y < 0.5f)
            {
                blackdie.transform.position = BlackDieRespawn.transform.position;
            }
        }

        //stuck in board
        foreach (InsideTrigger it in its)
        {
            if (it.name != null)
            {
                foreach (GameObject p in pieces)
                {
                    if (it.name == p.name)
                    {
                        if (aistate.board[int.Parse(p.name) - 1] == 0)
                        {
                            p.transform.position = PieceRespawn[int.Parse(p.gameObject.name) - 1].transform.position;
                            it.name = null;
                        }
                        else
                        {
                            p.transform.position = boardspots[aistate.board[int.Parse(p.name) - 1]].transform.position;
                            it.name = null;
                        }
                    }
                }
                foreach (GameObject die in dice)
                {
                    if (it.name == die.name)
                    {
                        die.transform.position = DieRespawn.transform.position;
                        it.name = null;
                    }
                }
            }
        }
    }
}
