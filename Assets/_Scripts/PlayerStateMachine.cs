using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour {

    public PLAYER_STATES state;
    public GameObject GameManager;
    public AIAnimationStateMachine aistate;
    public GameObject Leap;
    public PlayerHands playerhands;

    public bool canmove;
    public bool canroll;

    public bool piece_placed;
    public bool dice_rolled;
    public bool reroll;

    public GameObject boolLight, quadLight;
    public GameObject piece1Light, piece2Light, piece3Light, piece4Light, piece5Light;

    // implementing states as enumeration
    public enum PLAYER_STATES
    {
        S_DELAY,
        S_WAITING,
        S_ROLLING,
        S_GRABBING,
    };

    // Use this for initialization
    void Start () {
        state = PLAYER_STATES.S_DELAY;
        GameManager = this.gameObject;
        aistate = GameManager.GetComponent<AIAnimationStateMachine>();
        playerhands = Leap.GetComponent<PlayerHands>();

        piece_placed = false;
        reroll = false;

        boolLight.SetActive(false);
        quadLight.SetActive(false);
        piece1Light.SetActive(true);
        piece2Light.SetActive(true);
        piece3Light.SetActive(true);
        piece4Light.SetActive(true);
        piece5Light.SetActive(true);
        piece1Light.GetComponent<Light>().intensity = 0;
        piece2Light.GetComponent<Light>().intensity = 0;
        piece3Light.GetComponent<Light>().intensity = 0;
        piece4Light.GetComponent<Light>().intensity = 0;
        piece5Light.GetComponent<Light>().intensity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case PLAYER_STATES.S_DELAY:
                //player cannot pick up anything
                canmove = false;
                canroll = false;

                break;

            case PLAYER_STATES.S_WAITING:
                //player cannot pick up anything
                canmove = false;
                canroll = false;
                boolLight.GetComponent<Light>().intensity = Mathf.Lerp(boolLight.GetComponent<Light>().intensity, 0, 0.01f);
                quadLight.GetComponent<Light>().intensity = Mathf.Lerp(quadLight.GetComponent<Light>().intensity, 0, 0.01f);

                // EXIT: if AI is done playing
                if (!aistate.aiturn)
                    state = PLAYER_STATES.S_ROLLING;
                break;

            case PLAYER_STATES.S_ROLLING:
                //player can pick up dice not pieces
                canroll = true;
                canmove = false;
                boolLight.SetActive(false);
                quadLight.SetActive(false);

                // EXIT: die have landed with value
                // if player cant move any piece
                //state = PLAYER_STATES.S_WAITING;
                //break;

                // else player moves piece
                if (dice_rolled)
                {
                    dice_rolled = false;
                    state = PLAYER_STATES.S_GRABBING;
                    boolLight.SetActive(true);
                    quadLight.SetActive(true);
                    boolLight.GetComponent<Light>().intensity = quadLight.GetComponent<Light>().intensity = 0;
                }
                break;

            case PLAYER_STATES.S_GRABBING:
                //player can pick up pieces but not dice
                canmove = true;
                canroll = false;
                boolLight.GetComponent<Light>().intensity = Mathf.Lerp(boolLight.GetComponent<Light>().intensity, 1, 0.01f);
                quadLight.GetComponent<Light>().intensity = Mathf.Lerp(quadLight.GetComponent<Light>().intensity, 1, 0.01f);

                // EXIT: if piece has been moved to a correct placement on board
                if (piece_placed)
                {
                    // if players pieces are all in spot 15, player wins
                    if (aistate.pieces[0] == 15 && aistate.pieces[1] == 15 && aistate.pieces[2] == 15 && aistate.pieces[3] == 15 && aistate.pieces[4] == 15)
                    {
                        Debug.Log("Player Wins");
                        Time.timeScale = 0;
                    }
                    if (reroll)
                    {
                        state = PLAYER_STATES.S_WAITING;
                        aistate.aiturn = false;
                        reroll = false;
                        boolLight.GetComponent<Light>().intensity = quadLight.GetComponent<Light>().intensity = 1;
                    }
                    else
                    {
                        state = PLAYER_STATES.S_WAITING;
                        aistate.aiturn = true;
                        boolLight.GetComponent<Light>().intensity = quadLight.GetComponent<Light>().intensity = 1;
                    }
                    piece_placed = false;
                }
                break;
        }

        // Piece Lights
        if (playerhands.firstturn_swallow)
        {
            piece1Light.SetActive(true);
            if (piece1Light.GetComponent<Light>().intensity <= 0.95f)
                piece1Light.GetComponent<Light>().intensity = Mathf.Lerp(piece1Light.GetComponent<Light>().intensity, 1, 0.05f);
            else
                piece1Light.GetComponent<Light>().intensity = 1;
        } else {
            piece1Light.GetComponent<Light>().intensity = Mathf.Lerp(piece1Light.GetComponent<Light>().intensity, 0, 0.05f);
            if (piece1Light.GetComponent<Light>().intensity <= 0.05f)
            {
                piece1Light.SetActive(false);
            }
        }

        if (playerhands.firstturn_stormbird)
        {
            piece2Light.SetActive(true);
            if (piece2Light.GetComponent<Light>().intensity <= 0.95f)
                piece2Light.GetComponent<Light>().intensity = Mathf.Lerp(piece2Light.GetComponent<Light>().intensity, 1, 0.05f);
            else
                piece2Light.GetComponent<Light>().intensity = 1;
        } else {
            piece2Light.GetComponent<Light>().intensity = Mathf.Lerp(piece2Light.GetComponent<Light>().intensity, 0, 0.05f);
            if (piece2Light.GetComponent<Light>().intensity <= 0.05f)
            {
                piece2Light.SetActive(false);
            }
        }

        if (playerhands.firstturn_raven)
        {
            piece3Light.SetActive(true);
            if (piece3Light.GetComponent<Light>().intensity <= 0.95f)
                piece3Light.GetComponent<Light>().intensity = Mathf.Lerp(piece3Light.GetComponent<Light>().intensity, 1, 0.05f);
            else
                piece3Light.GetComponent<Light>().intensity = 1;
        } else {
            piece3Light.GetComponent<Light>().intensity = Mathf.Lerp(piece3Light.GetComponent<Light>().intensity, 0, 0.05f);
            if (piece3Light.GetComponent<Light>().intensity <= 0.05f)
            {
                piece3Light.SetActive(false);
            }
        }

        if (playerhands.firstturn_rooster)
        {
            piece4Light.SetActive(true);
            if (piece4Light.GetComponent<Light>().intensity <= 0.95f)
                piece4Light.GetComponent<Light>().intensity = Mathf.Lerp(piece4Light.GetComponent<Light>().intensity, 1, 0.05f);
            else
                piece4Light.GetComponent<Light>().intensity = 1;
        } else {
            piece4Light.GetComponent<Light>().intensity = Mathf.Lerp(piece4Light.GetComponent<Light>().intensity, 0, 0.05f);
            if (piece4Light.GetComponent<Light>().intensity <= 0.05f)
            {
                piece4Light.SetActive(false);
            }
        }

        if (playerhands.firstturn_eagle)
        {
            piece5Light.SetActive(true);
            if (piece5Light.GetComponent<Light>().intensity <= 0.95f)
                piece5Light.GetComponent<Light>().intensity = Mathf.Lerp(piece5Light.GetComponent<Light>().intensity, 1, 0.05f); 
            else
                piece5Light.GetComponent<Light>().intensity = 1;
        } else {
            piece5Light.GetComponent<Light>().intensity = Mathf.Lerp(piece5Light.GetComponent<Light>().intensity, 0, 0.05f);
            if (piece5Light.GetComponent<Light>().intensity <= 0.05f)
            {
                piece5Light.SetActive(false);
            }
        }
    }
}
