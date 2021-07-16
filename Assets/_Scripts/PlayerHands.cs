using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class PlayerHands : MonoBehaviour {

    LeapProvider provider;
    Frame frame;
    Hand hand;
    List<Finger> fingers;

    public GameObject Altar;
    public GameObject[] pieces;
    public Transform[] boardspots;
    public GameObject[] highlights;
    public float intensity;
    public GameObject[] respawnspots;
    public GameObject[] aipieces;

    public bool firstturn_swallow;
    public bool firstturn_stormbird;
    public bool firstturn_raven;
    public bool firstturn_rooster;
    public bool firstturn_eagle;
    public bool validmove;
    public bool opponentknocked;
    public bool reroll;
    public int dropped_piece;
    public int dropped_spot;
    public int diequad;
    public int diebool;

    public GameObject[] dice;
    public int last_piece;
    public bool dice_thrown;
    public bool diequad_thrown, diebool_thrown;
    int winA, winB, winnerquad, winnerbool;

    public GameObject boolLight;
    public GameObject quadLight;
    public Texture num0, num1, num2, num3, num4;
    public GameObject piece1Light, piece2Light, piece3Light, piece4Light, piece5Light;

    public GameObject GameManager;
    public AIAnimationStateMachine aistate;
    public PlayerStateMachine playerstate;

    // Use this for initialization
    void Start() {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        fingers = new List<Finger>();

        firstturn_swallow = true;
        firstturn_stormbird = true;
        firstturn_raven = true;
        firstturn_rooster = true;
        firstturn_eagle = true;
        validmove = false;

        last_piece = 1;
        dice_thrown = false;
        diequad_thrown = false;
        diebool_thrown = false;
        winA = -1;
        winB = -1;
        winnerquad = -1;
        winnerbool = -1;

        aistate = GameManager.GetComponent<AIAnimationStateMachine>();
        playerstate = GameManager.GetComponent<PlayerStateMachine>();

        int count = 0;
        boardspots = new Transform[16];
        for (int i = 0; i < aistate.gameboardlabels.transform.childCount; i++)
        {
            if (aistate.gameboardlabels.transform.GetChild(i).tag == "warspot" || aistate.gameboardlabels.transform.GetChild(i).tag == "playerspot")
            {
                boardspots[count] = aistate.gameboardlabels.transform.GetChild(i);
                count++;
            }
        }

        for (int i = 0; i < highlights.Length; i++)
        {
            highlights[i].SetActive(false);
            highlights[i].GetComponent<Renderer>().material.SetColor("_TintColor", Color.yellow); //Debug purposes
        }
    }

    // Update is called once per frame
    void Update() {
        frame = provider.CurrentFrame;
        if (frame.Hands.Count > 0) // hands in view
        {
            hand = frame.Hands[0];
            fingers = provider.CurrentFrame.Hands[0].Fingers;

            // Piece Grab (pinch gesture)
            if (provider.CurrentFrame.Hands[0].PinchDistance < 30f && playerstate.canmove) // player is pinching (and can grab piece)
            {
                Vector3 finger_pos = new Vector3(fingers[1].TipPosition.x, fingers[1].TipPosition.y, fingers[1].TipPosition.z);
                float r = 0.03f;
                int count = 0;
                foreach (GameObject piece in pieces)
                {
                    //grab piece
                    if (Vector3.Magnitude(finger_pos - piece.transform.position) <= r && aistate.pieces[int.Parse(piece.name) - 1] != 15) // piece is close to players pinch
                    {
                        RaycastHit hit;
                        last_piece = count + 1; // which piece was last touched (i.e. dropped)
                        if (!piece.GetComponent<Rigidbody>().isKinematic)
                            piece.GetComponent<Rigidbody>().isKinematic = true;

                        piece.transform.position = (finger_pos + new Vector3(fingers[0].TipPosition.x, fingers[0].TipPosition.y, fingers[0].TipPosition.z))/2;

                        if (Physics.Raycast(piece.transform.position, Vector3.down, out hit) || Physics.Raycast(piece.transform.position, Vector3.up, out hit))
                        {
                            if (hit.collider.tag == "gameboard")
                            {
                                for (int i = 0; i < highlights.Length; i++)
                                {
                                    highlights[i].SetActive(false);
                                }

                                //Debug.Log("Piece name: " + piece.name);
                                //Debug.Log("p: " + aistate.board[int.Parse(piece.name)-1]); // <--- piece on board

                                //*************************** GAME LOGIC ***************************\\
                                int newspot, newspot2, diequad2;
                                newspot = newspot2 = diequad2 = -99;
                                opponentknocked = false;
                                reroll = false;
                                dropped_piece = -1;
                                dropped_spot = -1;

                                int p = aistate.pieces[int.Parse(piece.name)-1]; // value of piece
                                int highlightedspot = int.Parse(hit.collider.gameObject.name);
                                // generate the 1 or 2 possible placement for piece grabbed
                                newspot = p + diequad;
                                if (diebool == 1)
                                {
                                    if (diequad == 1)
                                        diequad2 = 5;
                                    else if (diequad == 2)
                                        diequad2 = 6;
                                    else if (diequad == 3)
                                        diequad2 = 7;
                                    else if (diequad == 4)
                                        diequad2 = 10;
                                    newspot2 = p + diequad2;
                                }
                                else
                                {
                                    diequad2 = -99;
                                    newspot2 = -99;
                                }
                                /*Debug.Log("highlightedspot: " + highlightedspot);
                                Debug.Log("diebool: " + diebool);
                                Debug.Log("diequad: " + diequad);
                                if (diequad2 != -99)
                                    Debug.Log("diequad2: " + diequad2);
                                Debug.Log("newspot: " + newspot);
                                if (newspot2 != -99)
                                    Debug.Log("newspot2: " + newspot2);*/

                                
                                if (p == 0)
                                {
                                    if (piece.tag == "swallow")
                                    {
                                        if (firstturn_swallow)
                                        {
                                            if (highlightedspot == 4 && diequad == 2)
                                            {
                                                //another playerpiece is there
                                                if (aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                                {
                                                    /*[invalid move]*/
                                                    validmove = false;
                                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                                }
                                                else
                                                {
                                                    //[valid move]
                                                    validmove = true;
                                                    opponentknocked = false;
                                                    reroll = true;
                                                    dropped_piece = int.Parse(piece.name); //1
                                                    dropped_spot = int.Parse(hit.collider.gameObject.name); //4
                                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                }
                                            }
                                            else
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                        }
                                        else
                                        {
                                            if ((highlightedspot == 3 || highlightedspot == 7 || highlightedspot == 13) && diequad == 2)
                                            {
                                                //another playerpiece is there
                                                if (aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                                {
                                                    /*[invalid move]*/
                                                    validmove = false;
                                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                                }
                                                //an enemy is there (only applies to spot 7
                                                else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                                {
                                                    //[valid move]
                                                    validmove = true;
                                                    opponentknocked = true;
                                                    reroll = false;
                                                    dropped_piece = int.Parse(piece.name); //1
                                                    dropped_spot = int.Parse(hit.collider.gameObject.name); //7
                                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                }
                                                //spot is empty
                                                else
                                                {
                                                    //[valid move]
                                                    validmove = true;
                                                    opponentknocked = false;
                                                    reroll = false;
                                                    dropped_piece = int.Parse(piece.name); //1
                                                    dropped_spot = int.Parse(hit.collider.gameObject.name); //3,7,13
                                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                }

                                            }
                                            else
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                        }
                                    }
                                    else if (piece.tag == "stormbird" && firstturn_stormbird)
                                    {
                                        if (diequad == 1 && diebool == 1 && highlightedspot == newspot2)
                                        {
                                            // check if another friendly piece is there
                                            if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                            // check if enemy piece is there
                                            else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = true;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                            else {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = false;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name); //2
                                                dropped_spot = int.Parse(hit.collider.gameObject.name); //5
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                                {
                                                    opponentknocked = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }
                                    }
                                    else if (piece.tag == "raven" && firstturn_raven)
                                    {
                                        if (diequad == 2 && diebool == 1 && highlightedspot == newspot2)
                                        {
                                            // check if another friendly piece is there
                                            if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                            // check if enemy piece is there
                                            else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = true;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                            else
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = false;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name); //3
                                                dropped_spot = int.Parse(hit.collider.gameObject.name); //6
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                                {
                                                    opponentknocked = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }
                                    }
                                    else if (piece.tag == "rooster" && firstturn_rooster)
                                    {
                                        if (diequad == 3 && diebool == 1 && highlightedspot == newspot2)
                                        {
                                            // check if another friendly piece is there
                                            if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                            // check if enemy piece is there
                                            else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = true;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                            else
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = false;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name); //4
                                                dropped_spot = int.Parse(hit.collider.gameObject.name); //7
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                                {
                                                    opponentknocked = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }
                                    }
                                    else if (piece.tag == "eagle" && firstturn_eagle)
                                    {
                                        if (diequad == 4 && diebool == 1 && highlightedspot == newspot2)
                                        {
                                            // check if another friendly piece is there
                                            if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                            // check if enemy piece is there
                                            else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = true;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                            else
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = false;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name); //5
                                                dropped_spot = int.Parse(hit.collider.gameObject.name); //10
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                                if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                                {
                                                    opponentknocked = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }
                                    }
                                    else
                                    {
                                        // check if another friendly piece is there
                                        if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                        {
                                            // check if friendly rossette
                                            if (highlightedspot == 4 || highlightedspot == 8 || highlightedspot == 14)
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = false;
                                                reroll = true;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                            // check if friendly occupys any spot
                                            else
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }
                                        }
                                        // check if enemy piece is there
                                        else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                        {
                                            // check if war rossette
                                            if (highlightedspot == 8)
                                            {
                                                /*[invalid move]*/
                                                validmove = false;
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                            }

                                            // check if enemy occupys war spot
                                            else
                                            {
                                                //[valid move]
                                                validmove = true;
                                                opponentknocked = true;
                                                reroll = false;
                                                dropped_piece = int.Parse(piece.name);
                                                dropped_spot = int.Parse(hit.collider.gameObject.name);
                                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                                highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                            }
                                        }
                                        // spot is empty but is rossette
                                        else if (highlightedspot == 4 || highlightedspot == 8 || highlightedspot == 14)
                                        {
                                            //[valid move]
                                            validmove = true;
                                            opponentknocked = false;
                                            reroll = true;
                                            dropped_piece = int.Parse(piece.name);
                                            dropped_spot = int.Parse(hit.collider.gameObject.name);
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                        }
                                        // spot is empty with no special cases
                                        else
                                        {
                                            //[valid move]
                                            validmove = true;
                                            opponentknocked = false;
                                            reroll = false;
                                            dropped_piece = int.Parse(piece.name);
                                            dropped_spot = int.Parse(hit.collider.gameObject.name);
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                        }
                                    }
                                }
                                // check if the spot highlighted by user is a valid spot    
                                else if (highlightedspot == newspot || highlightedspot == newspot2) {
                                    // check if another friendly piece is there
                                    if (aistate.pieces[0] == highlightedspot || aistate.pieces[1] == highlightedspot || aistate.pieces[2] == highlightedspot || aistate.pieces[3] == highlightedspot || aistate.pieces[4] == highlightedspot)
                                    {
                                        // check if friendly rossette
                                        if (highlightedspot == 4 || highlightedspot == 8 || highlightedspot == 14)
                                        {
                                            //[valid move]
                                            validmove = true;
                                            opponentknocked = false;
                                            reroll = true;
                                            dropped_piece = int.Parse(piece.name);
                                            dropped_spot = int.Parse(hit.collider.gameObject.name);
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                        }
                                        //check if goal
                                        else if (highlightedspot == 15)
                                        {
                                            //[valid move]
                                            validmove = true;
                                            opponentknocked = false;
                                            reroll = false;
                                            dropped_piece = int.Parse(piece.name);
                                            dropped_spot = int.Parse(hit.collider.gameObject.name);
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                        }
                                        // check if friendly occupys any spot
                                        else
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }
                                    }
                                    // check if enemy piece is there
                                    else if (aistate.pieces[5] == highlightedspot || aistate.pieces[6] == highlightedspot || aistate.pieces[7] == highlightedspot || aistate.pieces[8] == highlightedspot || aistate.pieces[9] == highlightedspot)
                                    {
                                        // check if war rossette
                                        if (highlightedspot == 8)
                                        {
                                            /*[invalid move]*/
                                            validmove = false;
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                        }

                                        // check if enemy occupys war spot
                                        else
                                        {
                                            //[valid move]
                                            validmove = true;
                                            opponentknocked = true;
                                            reroll = false;
                                            dropped_piece = int.Parse(piece.name);
                                            dropped_spot = int.Parse(hit.collider.gameObject.name);
                                            highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                            highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                        }
                                    }
                                    // spot is empty but is rossette
                                    else if (highlightedspot == 4 || highlightedspot == 8 || highlightedspot == 14)
                                    {
                                        //[valid move]
                                        validmove = true;
                                        opponentknocked = false;
                                        reroll = true;
                                        dropped_piece = int.Parse(piece.name);
                                        dropped_spot = int.Parse(hit.collider.gameObject.name);
                                        highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                        highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                    }
                                    // spot is empty with no special cases
                                    else
                                    {
                                        //[valid move]
                                        validmove = true;
                                        opponentknocked = false;
                                        reroll = false;
                                        dropped_piece = int.Parse(piece.name);
                                        dropped_spot = int.Parse(hit.collider.gameObject.name);
                                        highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                        highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                    }
                                }
                                // otherwise highlight red
                                else
                                {
                                    //[invalid move]
                                    validmove = false;
                                    highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                    highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
                                }
                                //highlights[int.Parse(hit.collider.gameObject.name)].SetActive(true);
                                //highlights[int.Parse(hit.collider.gameObject.name)].GetComponent<Renderer>().material.SetColor("_TintColor", Color.white);
                                //validmove = true;
                                //******************************************************************\\
                            }
                            else
                            {
                                for (int i = 0; i < highlights.Length; i++)
                                {
                                    highlights[i].SetActive(false);
                                }
                            }
                        }
                        break; // only grab one piece at a time
                    }
                    count++;
                }
            }
            else
            {
                int count = 0;
                foreach (GameObject piece in pieces) //might not need loop anymore!
                {
                    RaycastHit hit;
                    if (piece.GetComponent<Rigidbody>().isKinematic)
                        piece.GetComponent<Rigidbody>().isKinematic = false;

                    if (Physics.Raycast(piece.transform.position, Vector3.down, out hit) || Physics.Raycast(piece.transform.position, Vector3.up, out hit))
                    {
                        if (hit.collider.tag == "gameboard")
                        {
                            if (validmove)
                            {
                                //check for first move boolean
                                if (aistate.pieces[dropped_piece-1] == 0)
                                {
                                    if (dropped_piece == 1 && firstturn_swallow)
                                    {
                                        firstturn_swallow = false;
                                    }
                                    else if (dropped_piece == 2 && firstturn_stormbird)
                                    {
                                        firstturn_stormbird = false;
                                    }
                                    else if (dropped_piece == 3 && firstturn_raven)
                                    {
                                        firstturn_raven = false;
                                    }
                                    else if (dropped_piece == 4 && firstturn_rooster)
                                    {
                                        firstturn_rooster = false;
                                    }
                                    else if (dropped_piece == 5 && firstturn_eagle)
                                    {
                                        firstturn_eagle = false;
                                    }
                                }
                                piece.transform.position = new Vector3(hit.collider.gameObject.transform.position.x, piece.transform.position.y, hit.collider.gameObject.transform.position.z);
                                aistate.pieces[dropped_piece - 1] = dropped_spot;//aistate.board[int.Parse(piece.gameObject.name) - 1] = int.Parse(hit.collider.gameObject.name);
                                if (opponentknocked)
                                {
                                    for (int j = 5; j < 10; j++)  //5-9 = black (AI) pieces
                                    {
                                        if (aistate.pieces[j] == dropped_spot)
                                        {
                                            if (aistate.pieces[j] >= 5 && aistate.pieces[j] <= 12)
                                            {
                                                aistate.pieces[j] = 0;
                                                aipieces[j - 5].transform.position = respawnspots[j].transform.position;
                                                break;
                                            }
                                        }
                                    }
                                }
                                /*if (count + 1 == last_piece)
                                {
                                    Debug.Log("piece placed");
                                    playerstate.piece_placed = true;
                                    last_piece = -1;
                                }*/
                                highlights[int.Parse(hit.collider.gameObject.name)].SetActive(false);
                                validmove = false;
                                if (reroll)
                                {
                                    playerstate.reroll = true;
                                    Debug.Log("reroll");
                                }
                                else
                                    playerstate.reroll = false;

                                playerstate.piece_placed = true;
                                //move pieces in incorrect spots back after making a move
                                for (int i = 0; i < pieces.Length; i++)
                                {
                                    if (pieces[i].transform.position.x != boardspots[aistate.pieces[i]].transform.position.x || pieces[i].transform.position.z != boardspots[aistate.pieces[i]].transform.position.z)
                                    {
                                        if (!(i == dropped_piece-1))
                                        {
                                            if (aistate.pieces[i] == 0)
                                            {
                                                Replace(pieces[i], respawnspots[i].transform);
                                                //pieces[i].transform.position = respawnspots[i].transform.position;
                                            }
                                            else if (aistate.pieces[i] == 15)
                                            {
                                                //do nothing
                                            }
                                            else
                                            {
                                                Replace(pieces[i], boardspots[aistate.pieces[i]]);
                                                //pieces[i].transform.position = boardspots[aistate.board[i]].transform.position;
                                            }
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                    count++;
                }

            }

            // Dice Roll (fist gesture)
            if (provider.CurrentFrame.Hands[0].GrabStrength > 0.75f && playerstate.canroll)
            {
                Vector3 finger_pos = new Vector3(fingers[1].TipPosition.x, fingers[1].TipPosition.y, fingers[1].TipPosition.z);
                float r = 0.1f;
                Vector velocity = hand.PalmVelocity; //used for speed of rotating dice
                foreach (GameObject die in dice)
                {
                    //grab piece
                    if (Vector3.Magnitude(finger_pos - die.transform.position) <= r)
                    {
                        die.GetComponent<Rigidbody>().isKinematic = true;
                        die.transform.rotation *= (Quaternion.AngleAxis(Random.Range(0.01f, velocity.Magnitude * 20), new Vector3(velocity.Magnitude, velocity.Magnitude, velocity.Magnitude).normalized));
                        die.transform.position = new Vector3(fingers[2].TipPosition.x, fingers[2].TipPosition.y, fingers[2].TipPosition.z);

                        if (finger_pos.y - Altar.transform.position.y > 0.65f) //check that player didnt drop dice prematurely while trying to lift/pick them up
                        {
                            dice_thrown = true;
                            diequad_thrown = false;
                            diebool_thrown = false;
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject die in dice)
                {
                    if (die.GetComponent<Rigidbody>().isKinematic)
                        die.GetComponent<Rigidbody>().isKinematic = false;
                }
                if (dice_thrown)
                {
                    RaycastHit hit;
                    foreach (GameObject die in dice)
                    {
                        if (Physics.Raycast(die.transform.position, Vector3.down, out hit, 0.01f)) //if dice is touching surface
                        {
                            if (hit.collider.tag == "altar")
                            {
                                if (die.GetComponent<Rigidbody>().velocity.magnitude < 0.0001f)
                                {
                                    winA = -1;
                                    winB = -1;
                                    if (!diequad_thrown)
                                        winnerquad = -1;
                                    if (!diebool_thrown)
                                        winnerbool = -1;
                                    if (die.name == "DieBool")
                                    {
                                        if (die.transform.GetChild(0).transform.position.y > die.transform.GetChild(1).transform.position.y)
                                            winA = 0;
                                        else
                                            winA = 1;
                                        if (die.transform.GetChild(2).transform.position.y > die.transform.GetChild(3).transform.position.y)
                                            winB = 2;
                                        else
                                            winB = 3;

                                        if (die.transform.GetChild(winA).transform.position.y > die.transform.GetChild(winB).transform.position.y)
                                            winnerbool = winA;
                                        else
                                            winnerbool = winB;

                                        if (winnerbool == 0 || winnerbool == 1)
                                        {
                                            winnerbool = 0;
                                            boolLight.GetComponent<Light>().cookie = num0;
                                        } else if (winnerbool == 2 || winnerbool == 3) {
                                            winnerbool = 1;
                                            boolLight.GetComponent<Light>().cookie = num1;
                                        } else {
                                            winnerbool = -1;
                                            Debug.Log("ERROR: diebool winner was <0 or >3");
                                        }
                                        //Debug.Log("DieBool Rolled: " + winnerbool);
                                        diebool_thrown = true;
                                    }
                                    else if (die.name == "Die") // 4 value die
                                    {
                                        if (die.transform.GetChild(0).transform.position.y > die.transform.GetChild(1).transform.position.y)
                                            winA = 0;
                                        else
                                            winA = 1;
                                        if (die.transform.GetChild(2).transform.position.y > die.transform.GetChild(3).transform.position.y)
                                            winB = 2;
                                        else
                                            winB = 3;

                                        if (die.transform.GetChild(winA).transform.position.y > die.transform.GetChild(winB).transform.position.y)
                                            winnerquad = winA;
                                        else
                                            winnerquad = winB;
                                        winnerquad++;
                                        if (winnerquad == 1)
                                        {
                                            quadLight.GetComponent<Light>().cookie = num1;
                                        }
                                        else if (winnerquad == 2)
                                        {
                                            quadLight.GetComponent<Light>().cookie = num2;
                                        }
                                        else if (winnerquad == 3)
                                        {
                                            quadLight.GetComponent<Light>().cookie = num3;
                                        }
                                        else if (winnerquad == 4)
                                        {
                                            quadLight.GetComponent<Light>().cookie = num4;
                                        }
                                        //Debug.Log("Die Rolled: " + winnerquad);
                                        diequad_thrown = true;
                                    }
                                    else
                                    {
                                        Debug.Log("ERROR: die check failed to identify die by name");
                                    }
                                }
                            }
                        }
                    }
                    if (diequad_thrown && diebool_thrown)
                    {
                        playerstate.dice_rolled = true;
                        dice_thrown = false;
                        diequad = winnerquad;
                        diebool = winnerbool;
                        bool[] firsts = { firstturn_swallow, firstturn_stormbird, firstturn_raven, firstturn_rooster, firstturn_eagle };
                        if (!AIScript.Helper.CanPlayerMove(aistate.pieces, firsts, diequad))
                        {
                            if (diebool == 1)
                            {
                                int quad2 = 0;
                                if (diequad == 1)
                                    quad2 = 5;
                                else if (diequad == 2)
                                    quad2 = 6;
                                else if (diequad == 3)
                                    quad2 = 7;
                                else if (diequad == 4)
                                    quad2 = 10;
                                else
                                    Debug.Log("Error diequad not 1-4");
                                if (!AIScript.Helper.CanPlayerMove(aistate.pieces, firsts, quad2))
                                {
                                    //skip turn
                                    Debug.Log("Player has no available move");
                                    aistate.aiturn = true;
                                    playerstate.state = PlayerStateMachine.PLAYER_STATES.S_WAITING;
                                    playerstate.dice_rolled = false;
                                }
                                else
                                {
                                    boolLight.SetActive(true);
                                    quadLight.SetActive(true);
                                }
                            }
                            else
                            {
                                //skip turn
                                Debug.Log("Player has no available move");
                                aistate.aiturn = true;
                                playerstate.state = PlayerStateMachine.PLAYER_STATES.S_WAITING;
                                playerstate.dice_rolled = false;
                            }
                        }
                        else
                        {
                            boolLight.SetActive(true);
                            quadLight.SetActive(true);
                        }

                    }
                }
            }
        }
        else //physics on, even when no hands in scene
        {
            foreach (GameObject die in dice)
            {
                if (die.GetComponent<Rigidbody>().isKinematic)
                    die.GetComponent<Rigidbody>().isKinematic = false;
            }
            foreach (GameObject piece in pieces) //might not need loop anymore!
            {
                if (piece.GetComponent<Rigidbody>().isKinematic)
                    piece.GetComponent<Rigidbody>().isKinematic = false;
            }
            for (int i = 0; i < highlights.Length; i++)
            {
                highlights[i].SetActive(false);
                highlights[i].GetComponent<Renderer>().material.SetColor("_TintColor", Color.yellow);
            }
        }
    }

    //would eventually want to use to lerp?
    void Replace(GameObject p, Transform respawn)
    {
        p.transform.position = respawn.transform.position;
    }
}

/*
 * GAME LOGIC:
 * 
 *  foreach piece:
 *      // generate the 1 or 2 possible placement for piece grabbed
 *      newspot = piece + diequad;
 *      if (diebool == 1)
 *          if (diequad == 1)
 *              diequad2 = 5;
 *          else if (diequad == 2)
 *              diequad2 = 6;
 *          else if (diequad == 3)
 *              diequad2 = 7;
 *          else if (diequad == 4)
 *              diequad2 = 10 ;    
 *          newspot2 = piece[i] + diequad2
 *      else
 *          diequad2 = null;
 *          newspot2 = null;
 *          
 *      // check if the spot highlighted by user is a valid spot    
 *      if (highlightedspot == newspot or newspot2) {
 *          // check if it is swallow's first move
 *          if (piece == 0 and piece is 'swallow') {
 *              if (diequad == 2)
 *                  piece = 4;
 *                  //skip aiturn (double roll)
 *                  //[valid move]
 *                  break;
 *              else
 *                  highlights.GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
 *                  //[invalid move]
 *                  break;
 *          
 *          // check if piece is there
 *          elif (otherpieces == highlightedspot)
 *              // check if friendly rossette
 *              if (highlightedspot == 4 or 14)
 *                  piece = highlightedspot;
 *                  //skip aiturn (double roll)
 *                  //[valid move]
 *                  break
 *              // check if war rossette
 *              elif (highlightedspot == 8)
 *                  // check if enemy occupys it
 *                  if (otherpiece == enemy)
 *                      highlights.GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
 *                      [invalid move]
 *                      break
 *                  // check if friendly occupys it
 *                  else
 *                      piece = 8
 *                      skip aiturn (double roll)
 *                      [valid move]
 *                      break
 *              // check if enemy occupys war spot
 *              elif (otherpiece = enemy)
 *                  otherpiece = 0
 *                  piece = highlightedspot
 *                  [valid move]
 *                  break
 *              // check if friendly occupys any spot
 *              else
 *                  highlight.GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
 *                  [invalid move]
 *                  break
 *          // should be good
 *          else
 *              piece = highlightedspot
 *              [valid move]
 *              break
 *      
 *      // otherwise highlight red
 *      else
 *          highlight.GetComponent<Renderer>().material.SetColor("_TintColor", Color.red);
 *          [invalid move]
 *          break
 * 
 * 
 * 
 *
 */
