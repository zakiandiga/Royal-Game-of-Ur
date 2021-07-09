using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimationStateMachine : MonoBehaviour {

    public static event Action<string> AI_TurnFinished;
    public static event Action<string> OnDiceThrownAI;

    public AI_STATES state;
    public Animator anim;

    public GameObject opponent;
    public GameObject gameboardlabels;
    private AIScript.AI ai;
    [Tooltip("Difficulty: Easiest (1) -> Hardest (inf)")]
    public int depth = 1;
    AIScript.AI.Move turn;
    int aipiece;

    public GameObject playercam;

    public int[] board; //int representation of boardpieces[] for AI script
    public GameObject[] boardpieces; //GameObject represenation of board[]
    public Transform[] boardspots; //positions of board spots
    public Transform[] dropspots;
    public GameObject[] blackdice; //AI dice GameObjects
    public GameObject[] piecerespawn;

    public GameObject aiboolLight;
    public GameObject aiquadLight;
    public Texture num0, num1, num2, num3, num4;
    public bool lerplighton, lerplightoff;

    public bool aiturn;
    private bool white; //is AI the white or black pieces?

    private IKControl ik;
    public GameObject ltarget;
    public GameObject rtarget;
    public Vector3 destination;
    public Vector3 restingdestination;
    public GameObject looktarget;
    public Vector3 lookdestination;

    public bool lefthand; //false=right hand
    public GameObject restingleft;
    public GameObject restingright;
    private float lerpoffset;

    //wrist offset
    public GameObject RightWrist;
    public GameObject LeftWrist;
    public GameObject RightHand;
    public GameObject LeftHand;
    private Vector3 RightIKOffset;
    private Vector3 LeftIKOffset;
    public float lerpspeed = 1f;

    public GameObject dietarget1;
    public GameObject dietarget2;
    public GameObject dietarget3;
    public bool rollingdie;
    public float die_lerpspped = 3f;
    public float look_lerpspeed = 1f;

    public GameObject destoffset;
    public GameObject restoffset;
    public GameObject dest;
    public GameObject rest;

    // implementing states as enumeration
    public enum AI_STATES {
        S_WAITING,
        S_IKtoDICEGRAB,
        S_IKtoDICETHROW1,
        S_IKtoDICETHROW2,
        S_IKtoDICETHROW3,
        S_IKtoDICETHROW4,
        S_DiceFallWaiting,
        S_DiceResultChecking, //Added this state for new dice roll system
        S_CALCULATETURN,
        S_IKtoPIECEGRAB, //Renamed from grab to piecegrab
        S_PIECEGRABBING, //Renamed to PIECEGRABBING
        S_IKtoDROP,
        S_PIECEDROPPING, //Renamed to PIECEDROPPING
        S_IKtoWAIT,
        S_AI_Error //Debug
    };

    #region PlayingProperties
    private int numberDiceResult, boolDiceResult, totalDiceResult;
    private bool isCheckingDiceResult = false;
    private bool numDiceRolled = false;
    private bool boolDiceRolled = false;

    #endregion

    void Start() {
        //initialization of game
        anim.SetBool("LeftGrab", false);
        anim.SetBool("RightGrab", false);

        ai = new AIScript.AI();
        turn = new AIScript.AI.Move(0, 0);
        aiturn = false;

        board = new int[10];
        // 0-4:White & 5-9:Black 
        for (int i = 0; i < 10; i++)
        {
            board[i] = 0;
        }

        int count = 0;
        boardspots = new Transform[16];
        for (int i = 0; i < gameboardlabels.transform.childCount; i++)
        {
            if (gameboardlabels.transform.GetChild(i).tag == "warspot" || gameboardlabels.transform.GetChild(i).tag == "opponentspot")
            {
                boardspots[count] = gameboardlabels.transform.GetChild(i);
                count++;
            }
        }

        aiquadLight.SetActive(false);
        aiboolLight.SetActive(false);
        aiboolLight.GetComponent<Light>().intensity = 0;
        aiquadLight.GetComponent<Light>().intensity = 0;
        lerplighton = lerplightoff = false;

        ik = opponent.GetComponent<IKControl>();
        white = false;

        ltarget.transform.position = restingleft.transform.position;
        rtarget.transform.position = restingright.transform.position;
        looktarget.transform.position = playercam.transform.position;
        //wristoffset = new Vector3(-0.05f,0.1f,-0.05f);
        RightIKOffset = RightWrist.transform.position - RightHand.transform.position; //RightHand.transform.localPosition / 10;// RightWrist.transform.TransformPoint(RightHand.transform.localPosition) / 10;
        LeftIKOffset = LeftWrist.transform.position - LeftHand.transform.position; //LeftHand.transform.localPosition / 10;// LeftWrist.transform.TransformPoint(LeftHand.transform.localPosition) / 10;
        //gA = gB+(A-B)
        //destination(a) = destination(b) + (wrist.pos-palm.pos)
        //A-wrist, B-palm

        if (lefthand)
        {
            destination = restingleft.transform.position + LeftIKOffset;
            restingdestination = restingright.transform.position + RightIKOffset;
        }
        else
        {
            destination = restingright.transform.position + RightIKOffset;
            restingdestination = restingleft.transform.position + LeftIKOffset;
        }
        lerpoffset = 0.01f;

        state = AI_STATES.S_WAITING;
        rollingdie = false;

        //DEBUG
        destoffset = new GameObject("destoffset");
        destoffset.AddComponent<BoxCollider>();
        destoffset.GetComponent<BoxCollider>().isTrigger = true;
        destoffset.GetComponent<BoxCollider>().size = new Vector3(.1f, .1f, .1f);
        restoffset = new GameObject("restoffset");
        restoffset.AddComponent<BoxCollider>();
        restoffset.GetComponent<BoxCollider>().isTrigger = true;
        restoffset.GetComponent<BoxCollider>().size = new Vector3(.1f, .1f, .1f);
        dest = new GameObject("dest");
        dest.AddComponent<BoxCollider>();
        dest.GetComponent<BoxCollider>().isTrigger = true;
        dest.GetComponent<BoxCollider>().size = new Vector3(.05f, .05f, .05f);
        rest = new GameObject("rest");
        rest.AddComponent<BoxCollider>();
        rest.GetComponent<BoxCollider>().isTrigger = true;
        rest.GetComponent<BoxCollider>().size = new Vector3(.05f, .05f, .05f);
    }

    private void OnEnable()
    {
        PhaseManager.OnPhaseChange += CheckTurn;
        DiceBehaviour.OnDiceBoolResult += DiceBoolResultCheck;
        DiceBehaviour.OnDiceNumberResult += DiceNumberResultCheck;
    }

    private void OnDisable()
    {
        PhaseManager.OnPhaseChange -= CheckTurn;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
    }

    private void CheckTurn(string playerPhase)
    {
        if (playerPhase == "Waiting" && !aiturn)
        {
            aiturn = true;
        }
    }

    #region DiceResultChecks
    private void DiceBoolResultCheck(int boolResult, bool aIDice)
    {
        if (aIDice && !boolDiceRolled)
        {
            boolDiceResult = boolResult;
            StartCoroutine(BoolDiceRollDelay());
        }
    }

    private IEnumerator BoolDiceRollDelay()
    {
        Debug.Log("AI: BoolDiceRollDelay() STARTED");
        float resultDelay = 1.2f;

        yield return new WaitForSeconds(resultDelay);
        boolDiceRolled = true;
    }

    private void DiceNumberResultCheck(int numResult, bool aIDice)
    {
        if(aIDice && !numDiceRolled)
        {
            numberDiceResult = numResult;
            StartCoroutine(NumDiceRollDelay());
        }        
    }

    private IEnumerator NumDiceRollDelay()
    {
        float resultDelay = 1.2f;

        yield return new WaitForSeconds(resultDelay);
        numDiceRolled = true;
    }

    private IEnumerator DiceThrowDelay()
    {
        float delay = 1.3f;
        yield return new WaitForSeconds(delay);

        OnDiceThrownAI?.Invoke("AI dice thrown");
        isCheckingDiceResult = false;
        state = AI_STATES.S_DiceResultChecking;

    }

    private int TotalDiceResultCheck()
    {
        if (boolDiceResult == 0)
        {
            return numberDiceResult;
        }
        else if (boolDiceResult == 1)
        {
            if (numberDiceResult >= 1 && numberDiceResult <= 3)
                return numberDiceResult + 4;
            else if (numberDiceResult == 4)
                return 10;
        }

        Debug.Log("AI: Total dice result ERROR");
        return -2;
        //Debug.Log("Total AI Dice result = " + totalDiceResult);
    }
    #endregion

    private void ResetDiceResult()
    {
        totalDiceResult = 0;
        boolDiceResult = 0;
        numberDiceResult = 0;

        numDiceRolled = false;
        boolDiceRolled = false;
    }

    void Update () {
        RightIKOffset = RightWrist.transform.position - RightHand.transform.position; //RightHand.transform.localPosition / 10;// RightWrist.transform.TransformPoint(RightHand.transform.localPosition) / 10;
        LeftIKOffset = LeftWrist.transform.position - LeftHand.transform.position; //LeftHand.transform.localPosition / 10;// LeftWrist.transform.TransformPoint(LeftHand.transform.localPosition) / 10;
        //Lerp
        looktarget.transform.position = Vector3.Lerp(looktarget.transform.position, lookdestination, Time.deltaTime * look_lerpspeed);
        if (rollingdie)
        {
            if (lefthand) {
                Debug.Log("Error, called rollingdie with left hand");
                //ltarget.transform.position = Vector3.Lerp(ltarget.transform.position, destination, Time.deltaTime * die_lerpspped);
            } else {
                rtarget.transform.position = Vector3.Lerp(rtarget.transform.position, destination, Time.deltaTime * die_lerpspped);
                ltarget.transform.position = Vector3.Lerp(ltarget.transform.position, restingdestination, Time.deltaTime * die_lerpspped);
            }
        }
        else
        {
            if (lefthand) {
                ltarget.transform.position = Vector3.Lerp(ltarget.transform.position, destination, Time.deltaTime * lerpspeed);
                if (restingdestination != null)
                {
                    rtarget.transform.position = Vector3.Lerp(rtarget.transform.position, restingdestination, Time.deltaTime * lerpspeed);
                }
            } else {
                rtarget.transform.position = Vector3.Lerp(rtarget.transform.position, destination, Time.deltaTime * lerpspeed);
                if (restingdestination != null)
                {
                    ltarget.transform.position = Vector3.Lerp(ltarget.transform.position, restingdestination, Time.deltaTime * lerpspeed);
                }
            }
        }

        if (lerplighton)
        {
            aiboolLight.GetComponent<Light>().intensity = Mathf.Lerp(aiboolLight.GetComponent<Light>().intensity, 1, 0.01f);
            aiquadLight.GetComponent<Light>().intensity = Mathf.Lerp(aiquadLight.GetComponent<Light>().intensity, 1, 0.01f);
        }
        else if (lerplightoff)
        {
            aiboolLight.GetComponent<Light>().intensity = Mathf.Lerp(aiboolLight.GetComponent<Light>().intensity, 0, 0.01f);
            aiquadLight.GetComponent<Light>().intensity = Mathf.Lerp(aiquadLight.GetComponent<Light>().intensity, 0, 0.01f);
        }

        switch (state)
        {
            #region stateWaiting
            case AI_STATES.S_WAITING: // Player's Turn
                lookdestination = playercam.transform.position;
                look_lerpspeed = 5f;
                aiboolLight.SetActive(false);
                aiquadLight.SetActive(false);
                if (lefthand)
                {
                    destination = restingleft.transform.position + LeftIKOffset;
                    restingdestination = restingright.transform.position + RightIKOffset;
                }
                else
                {
                    destination = restingright.transform.position + RightIKOffset;
                    restingdestination = restingleft.transform.position + LeftIKOffset;
                }
                if (aiturn)
                {
                    Debug.Log("AI: AI Turn Start!");
                    state = AI_STATES.S_IKtoDICEGRAB;
                }
                break;
            #endregion
            case AI_STATES.S_IKtoDICEGRAB: //AI dice rolling animation 1
                rollingdie = true;
                look_lerpspeed = 1f;
                lookdestination = playercam.transform.position;
                destination = blackdice[0].transform.position + RightIKOffset;
                lefthand = false;
                if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                {
                    anim.SetBool("RightGrab", true);
                    state = AI_STATES.S_IKtoDICETHROW1;
                }
                break;

            case AI_STATES.S_IKtoDICETHROW1: //AI dice rolling animation 2
                lookdestination = playercam.transform.position;
                destination = dietarget1.transform.position + RightIKOffset;
                foreach (GameObject blackdie in blackdice)
                {
                    blackdie.GetComponent<Rigidbody>().isKinematic = true;
                    blackdie.transform.position = RightHand.transform.position;
                }

                if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                {
                    anim.SetBool("RightGrab", false);
                    state = AI_STATES.S_IKtoDICETHROW2;
                }
                break;

            case AI_STATES.S_IKtoDICETHROW2: //AI dice rolling animation 3
                lookdestination = playercam.transform.position;
                destination = dietarget2.transform.position + RightIKOffset;
                foreach (GameObject blackdie in blackdice)
                {
                    blackdie.transform.position = RightHand.transform.position;
                }

                if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                {
                    state = AI_STATES.S_IKtoDICETHROW3;
                }
                break;

            case AI_STATES.S_IKtoDICETHROW3: //AI dice rolling animation 4
                lookdestination = playercam.transform.position;
                destination = dietarget1.transform.position + RightIKOffset;
                foreach (GameObject blackdie in blackdice)
                {
                    blackdie.transform.position = RightHand.transform.position;
                }

                if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                {
                    state = AI_STATES.S_IKtoDICETHROW4;
                }
                break;

            case AI_STATES.S_IKtoDICETHROW4: //AI dice rolling animation 5
                lookdestination = playercam.transform.position;
                destination = dietarget3.transform.position + RightIKOffset;

                if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                {
                    foreach (GameObject blackdie in blackdice)
                    {
                        float torqueX = UnityEngine.Random.Range(50, 80);
                        float torqueY = UnityEngine.Random.Range(50, 80);
                        float torqueZ = UnityEngine.Random.Range(50, 80);
                        Rigidbody rb = blackdie.GetComponent<Rigidbody>();
                        //DiceBehaviour db = blackdie.GetComponent<DiceBehaviour>();
                        rb.isKinematic = false;                        
                        rb.AddForce(new Vector3 (0,1,0), ForceMode.Impulse); //make a variable for the direction
                        rb.AddTorque(new Vector3 (torqueX, torqueY, torqueZ), ForceMode.Force);
                        //db.DiceThrowAI();
                    }
  
                    if(state != AI_STATES.S_DiceFallWaiting)
                        state = AI_STATES.S_DiceFallWaiting;
                    
                }
                else
                {
                    foreach (GameObject blackdie in blackdice)
                    {
                        blackdie.transform.position = RightHand.transform.position;                        
                    }
                }
                break;

            case AI_STATES.S_DiceFallWaiting:
                if(!isCheckingDiceResult)
                {
                    isCheckingDiceResult = true;
                    StartCoroutine(DiceThrowDelay());
                    Debug.Log("AI: Execute DiceThrowDelay()");                    
                }
                break;

            case AI_STATES.S_DiceResultChecking:                                
                if (boolDiceRolled && numDiceRolled)
                {
                    totalDiceResult = TotalDiceResultCheck();
                    if(totalDiceResult < 0)
                    {
                        Debug.Log("AI: Error dice result, AI cannot proceed");
                        state = AI_STATES.S_AI_Error;
                    }

                    if(totalDiceResult > 0)
                    {
                        Debug.Log("AI: Dice Result CHECKED, Result = " + totalDiceResult);
                        state = AI_STATES.S_CALCULATETURN;
                    }
                                    
                }
                break;
            case AI_STATES.S_AI_Error:
                break;

            #region stateCalculateTurn
            case AI_STATES.S_CALCULATETURN:
                Debug.Log("AI Calculating turn!"); //STATE MONITOR -Zak

                rollingdie = false;

                //turn = ai.NextMove(board, rollDie(), depth);
                turn = ai.NextMove(board, totalDiceResult, depth); //changed to the new dice roll system

                if (turn.destination == 0) //No available move for AI
                {
                    aiturn = false;
                    Debug.Log("AI Lost Turn");
                    destination = restingright.transform.position + RightIKOffset;


                    state = AI_STATES.S_WAITING;
                    aiturn = false;
                    lefthand = false;

                    ResetDiceResult();

                    AI_TurnFinished?.Invoke("Move Unavailable"); //Notify PhaseManager to switch to player turn
                    break;
                }
                aiboolLight.SetActive(true);
                aiquadLight.SetActive(true);
                lerplighton = true;
                lerplightoff = false;

                aipiece = turn.piece;
                if (!white)
                {
                    aipiece += 4;
                }
                if (aipiece >= 10)
                {
                    Debug.Log("ERROR: PIECE INDEX IS 10");
                }

                foreach (GameObject p in boardpieces)
                {
                    p.GetComponent<Rigidbody>().isKinematic = true;
                }
                
                //piece 1
                if (aipiece == 5)
                {
                    if (board[aipiece] == 0) // starting point off board
                    {
                        lefthand = true;
                    }
                    else if (board[aipiece] >= 1 && board[aipiece] <= 8)
                    {
                        if (turn.destination >= 10)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else
                    {
                        lefthand = false;
                    }
                }
                //piece 2
                else if (aipiece == 6)
                {
                    if (board[aipiece] == 0) // starting point off board
                    {
                        lefthand = true;
                    }
                    else if (board[aipiece] >= 1 && board[aipiece] <= 8)
                    {
                        if (turn.destination >= 10)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else
                    {
                        lefthand = false;
                    }
                }
                //piece 3
                else if (aipiece == 7)
                {
                    if (board[aipiece] == 0) // starting point off board
                    {
                        if (turn.destination > 8)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else if (board[aipiece] >= 1 && board[aipiece] <= 8)
                    {
                        if (turn.destination >= 10)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else
                    {
                        lefthand = false;
                    }
                }
                //piece 4
                else if (aipiece == 8)
                {
                    if (board[aipiece] == 0) // starting point off board
                    {
                        if (turn.destination >= 2 && turn.destination <= 7)
                        {
                            lefthand = true;
                        }
                        else
                        {
                            lefthand = false;
                        }
                    }
                    else if (board[aipiece] >= 1 && board[aipiece] <= 8)
                    {
                        if (turn.destination >= 10)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else
                    {
                        lefthand = false;
                    }
                }
                //piece 5
                else if (aipiece == 9)
                {
                    if (board[aipiece] == 0) // starting point off board
                    {
                        if (turn.destination >= 3 && turn.destination <= 6)
                        {
                            lefthand = true;
                        }
                        else
                        {
                            lefthand = false;
                        }
                    }
                    else if (board[aipiece] >= 1 && board[aipiece] <= 8)
                    {
                        if (turn.destination >= 10)
                        {
                            lefthand = false;
                        }
                        else
                        {
                            lefthand = true;
                        }
                    }
                    else
                    {
                        lefthand = false;
                    }
                }
                
                //set ikTargetObj = piece pos
                if (lefthand)
                {
                    restingdestination = restingright.transform.position + RightIKOffset;// +wristoffset;
                    destination = boardpieces[aipiece].transform.position + LeftIKOffset;
                }
                else
                {
                    restingdestination = restingleft.transform.position + LeftIKOffset;
                    destination = boardpieces[aipiece].transform.position + RightIKOffset;
                }
                /*if (board[aipiece] >= 1 && board[aipiece] <= 8)
                {
                    lefthand = true;
                    restingdestination = restingright.transform.position;// +wristoffset;
                }
                else if (board[aipiece] == 0 || (board[aipiece] > 8 && board[aipiece] <= 15))
                {
                    lefthand = false;
                    restingdestination = restingleft.transform.position;
                }
                else
                {
                    Debug.Log("ERROR: IK Obj Invalid index");
                }*/
                lookdestination = destination;
                state = AI_STATES.S_IKtoPIECEGRAB;
                break;
            #endregion

            #region statePieceMoving
            case AI_STATES.S_IKtoPIECEGRAB:
				if (lefthand)
				{
					if ((ltarget.transform.position - destination).magnitude < lerpoffset)
					{
                        anim.SetBool("LeftGrab", true);
                        state = AI_STATES.S_PIECEGRABBING;
					}
				}
				else
				{
					if ((rtarget.transform.position - destination).magnitude < lerpoffset)
					{
                        anim.SetBool("RightGrab", true);
                        state = AI_STATES.S_PIECEGRABBING;
					}
				}
				break;

            case AI_STATES.S_PIECEGRABBING:
                board[aipiece] = turn.destination;
                if (lefthand)
                {
                    destination = dropspots[turn.destination].transform.position + LeftIKOffset;
                }
                else
                {
                    destination = dropspots[turn.destination].transform.position + RightIKOffset;
                }
                lookdestination = destination;
                for (int i = 0; i < 5; i++)
                {
                    if (board[i] == turn.destination)
                    {
                        if (turn.destination > 4 && turn.destination < 13)
                        {
                            board[i] = 0;
                            boardpieces[i].transform.position = piecerespawn[i].transform.position;
                            Debug.Log("Opponent knocks player: board[" + i + "] = 0");
                            break;
                        }
                    }
                }
                state = AI_STATES.S_IKtoDROP;
                break;

			case AI_STATES.S_IKtoDROP:
				if (lefthand)
				{
                    boardpieces[aipiece].transform.position = LeftHand.transform.position;
                    if ((ltarget.transform.position - destination).magnitude < lerpoffset)
					{
                        boardpieces[aipiece].transform.position = dropspots[turn.destination].transform.position;//boardpieces[aipiece].transform.position = destination - LeftIKOffset;
                        anim.SetBool("LeftGrab", false);
                        state = AI_STATES.S_PIECEDROPPING;
					}
				}
				else
				{
					boardpieces[aipiece].transform.position = RightHand.transform.position;
                    if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                    {
                        boardpieces[aipiece].transform.position = dropspots[turn.destination].transform.position;//boardpieces[aipiece].transform.position = destination - RightIKOffset;
                        anim.SetBool("RightGrab", false);
                        state = AI_STATES.S_PIECEDROPPING;
					}
				}
				break;

            case AI_STATES.S_PIECEDROPPING:
                foreach (GameObject p in boardpieces)
                {
                    p.GetComponent<Rigidbody>().isKinematic = false;
                }
				if (lefthand)
				{
					destination = restingleft.transform.position + LeftIKOffset;// + wristoffset
				}
				else
				{
                    destination = restingright.transform.position + RightIKOffset;
				}
                lookdestination = playercam.transform.position;
                // if ai pieces are all in spot 15, player wins
                if (board[5] == 15 && board[6] == 15 && board[7] == 15 && board[8] == 15 && board[9] == 15)
                {
                    Debug.Log("AI Wins");
                    Time.timeScale = 0;
                }
                lerplighton = false;
                lerplightoff = true;
                state = AI_STATES.S_IKtoWAIT;                
                break;

            #endregion

            #region stateWait
            case AI_STATES.S_IKtoWAIT:
				if (lefthand)
				{
					if ((ltarget.transform.position - destination).magnitude < lerpoffset)
					{
                        //AI drop on the rosette
                        if (turn.destination == 4 || turn.destination == 8 || turn.destination == 14)
                        {
                            //AI reroll
                            aiturn = true;
                            lefthand = false;

                            ResetDiceResult();

                            state = AI_STATES.S_WAITING;                          
                        }
                        else
                        {
                            aiturn = false;
                            lefthand = false;

                            //Reset AI dice results to 0 after finish a turn
                            ResetDiceResult();

                            state = AI_STATES.S_WAITING;

                            AI_TurnFinished?.Invoke("PieceDropped"); //Notify PhaseManager to switch to player turn
                        }
                    }
				}
				else
				{
					if ((rtarget.transform.position - destination).magnitude < lerpoffset)
					{
                        if (turn.destination == 4 || turn.destination == 8 || turn.destination == 14)
                        {
                            //AI reroll
                            aiturn = true;
                            lefthand = false;

                            ResetDiceResult();

                            state = AI_STATES.S_WAITING;

                        }
                        else
                        {
                            aiturn = false;
                            lefthand = false;

                            //Reset AI dice results to 0 after finish a turn
                            ResetDiceResult();

                            state = AI_STATES.S_WAITING;

                            AI_TurnFinished?.Invoke("PieceDropped"); //Notify PhaseManager to switch to player turn

                        }
                    }
				}
                break;
                #endregion
        }

        //DEBUG
        destoffset.transform.position = destination;
        restoffset.transform.position = restingdestination;
        if (lefthand)
        {
            dest.transform.position = destination - LeftIKOffset;
            rest.transform.position = restingdestination - RightIKOffset;
        }
        else
        {
            dest.transform.position = destination - RightIKOffset;
            rest.transform.position = restingdestination - LeftIKOffset;
        }
        Debug.DrawLine(RightWrist.transform.position, RightHand.transform.position);
        Debug.DrawLine(LeftWrist.transform.position, LeftHand.transform.position);
	}

    int rollDie()
    {
        int die1 = UnityEngine.Random.Range(0, 2); //Modified to UnityEngine namespace
        int die2 = UnityEngine.Random.Range(1, 5);
        
        int tempdie = -1;
        if (die1 == 1)
        {
            if (die2 == 1)
            {
                tempdie = 5;
                aiquadLight.GetComponent<Light>().cookie = num1;
            }
            else if (die2 == 2)
            {
                tempdie = 6;
                aiquadLight.GetComponent<Light>().cookie = num2;
            }
            else if (die2 == 3)
            {
                tempdie = 7;
                aiquadLight.GetComponent<Light>().cookie = num3;
            }
            else if (die2 == 4)
            {
                tempdie = 10;
                aiquadLight.GetComponent<Light>().cookie = num4;
            }
            else
                Debug.Log("ERROR: DIE ROLL <0 or >5");
            Debug.Log("AI Rolled: " + die2 + " or " + tempdie);
            die2 = tempdie;

            aiboolLight.GetComponent<Light>().cookie = num1;
        }
        if (die1 == 0)
        {
            aiboolLight.GetComponent<Light>().cookie = num0;
            if (die2 == 1)
                aiquadLight.GetComponent<Light>().cookie = num1;
            else if (die2 == 2)
                aiquadLight.GetComponent<Light>().cookie = num2;
            else if (die2 == 3)
                aiquadLight.GetComponent<Light>().cookie = num3; 
            else if (die2 == 4)
                aiquadLight.GetComponent<Light>().cookie = num4;

            Debug.Log("AI Rolled: " + die2);
        }
        return die2;
    }

    // Linear travel (Daniel Kharlamov)
    public static Vector3 travelTo(Vector3 frm, Vector3 to, float speed)
    {
        Vector3 dir = Vector3.Normalize(to - frm);
        return frm + (dir * speed);
    }
}
