using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimationStateMachine : MonoBehaviour {

    public static event Action<string> AI_TurnFinished;
    public static event Action<string> OnDiceThrownAI;
    public static event Action<string> OnPieceDropAI;
 
    public AI_STATES state;
    public Animator anim;

    public GameObject opponent;
    public GameObject gameboardlabels;
    private AIScript.AI ai;
    AIScript.AI.Move turn;
    [Tooltip("Difficulty: Easiest (1) -> Hardest (inf)")]
    public int depth = 1;
    int currentTargetPiece;  //Zak: change the name from aipiece;

    public GameObject playercam;

    public int[] pieces; //int representation of boardpieces[] for AI script. Zak: change var name from board to pieces
    public GameObject[] boardpieces; //GameObject represenation of board[]
    public Transform[] boardspots; //positions of board spots
    public Transform[] dropspots;
    public GameObject[] blackdice; //AI dice GameObjects
    public GameObject[] piecerespawn;

    public GameObject aiboolLight;
    public GameObject aiquadLight;
    public Texture num0, num1, num2, num3, num4;
    public bool lerplighton, lerplightoff;

    public bool aiturn; //assigned on Start()
    private bool white; //assigned on Start()

    #region IKProperty
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
    #endregion

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
    //Dice
    private int numberDiceResult, boolDiceResult, totalDiceResult;
    private bool isCheckingDiceResult = false;
    private bool numDiceRolled = false;
    private bool boolDiceRolled = false;

    //Piece
    private GameObject currentTargetPieceGameObject;
    private PieceBehaviour currentPieceBehaviour; //Added this to adjust with new PieceBehaviour system
    private Rigidbody currentRigidBody; //use this instead of using for loop
    #endregion

    void Start() {
        //initialization of game
        anim.SetBool("LeftGrab", false);
        anim.SetBool("RightGrab", false);

        ai = new AIScript.AI();
        turn = new AIScript.AI.Move(0, 0);
        aiturn = false;

        pieces = new int[10];
        // 0-4:White & 5-9:Black 
        for (int i = 0; i < 10; i++) //All pieces start from 0 (starting/pieces' Spawn point)
        {
            pieces[i] = 0;
        }

        /* assigned on the inspector
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
        */

        aiquadLight.SetActive(false);
        aiboolLight.SetActive(false);
        aiboolLight.GetComponent<Light>().intensity = 0;
        aiquadLight.GetComponent<Light>().intensity = 0;
        lerplighton = lerplightoff = false;

        ik = opponent.GetComponent<IKControl>();
        white = false; //AI set to black piece

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
        PieceBehaviour.OnPieceBackToStart += KickedPiecesPositionUpdate;
    }

    private void OnDisable()
    {
        PhaseManager.OnPhaseChange -= CheckTurn;
        DiceBehaviour.OnDiceBoolResult -= DiceBoolResultCheck;
        DiceBehaviour.OnDiceNumberResult -= DiceNumberResultCheck;
        PieceBehaviour.OnPieceBackToStart -= KickedPiecesPositionUpdate;
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

    private void ResetDiceResult() //Called after AI finish its turn
    {
        totalDiceResult = 0;
        boolDiceResult = 0;
        numberDiceResult = 0;

        numDiceRolled = false;
        boolDiceRolled = false;
    }
    #endregion
    #region PiecePositionChecks
    private void PiecesLocationUpdate()
    {
        Debug.Log("PIECE LOCATION UPDATE");
        for (int i = 0; i<boardpieces.Length; i++)
        {
            pieces[i] = boardpieces[i].gameObject.GetComponent<PieceBehaviour>().currentSquare;
            Debug.Log(i + " is at " + pieces[i]);
        }
    }

    private void KickedPiecesPositionUpdate(GameObject kickedPiece)
    {
        for (int i = 0; i < boardpieces.Length; i++) //loop through all the pieces
        {
            if (boardpieces[i].gameObject == kickedPiece) //if it's the same one with the kickedPiece
            {
                pieces[i] = 0; //reset its int representation to the starting position
                Debug.Log("AI: RESET POSITION OF KICKED" + kickedPiece + " (piece number " + i +") to its starting position");

            }
        }
    }

    #endregion


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
                if(currentPieceBehaviour != null)
                {
                    currentPieceBehaviour = null;
                }
                if(currentRigidBody != null)
                {
                    currentRigidBody = null;
                }

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
                    PiecesLocationUpdate();
                    state = AI_STATES.S_IKtoDICEGRAB;
                }
                break;
            #endregion
            #region stateDiceThrow
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
                }
                break;

            case AI_STATES.S_DiceResultChecking:
                if (boolDiceRolled && numDiceRolled)
                {
                    totalDiceResult = TotalDiceResultCheck();
                    if (totalDiceResult < 0)
                    {
                        Debug.Log("AI: Error dice result, AI cannot proceed");

                    }

                    if (totalDiceResult > 0)
                    {
                        Debug.Log("AI: Dice Result CHECKED, Result = " + totalDiceResult);
                        state = AI_STATES.S_CALCULATETURN;
                    }
                }
                break;
            #endregion

            #region stateCalculateTurn
            case AI_STATES.S_CALCULATETURN:
                Debug.Log("AI Calculating turn!"); //STATE MONITOR -Zak
                rollingdie = false;

                //turn = ai.NextMove(board, rollDie(), depth);
                turn = ai.NextMove(pieces, totalDiceResult, depth); //changed to the new dice roll system

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

                int tempTargetPiece = turn.piece;

                /* assign a rb variable instead and
                 * change the rigidbody isKinematic during AI_STATES.S_IKtoDROP
                foreach (GameObject p in boardpieces)
                {
                    p.GetComponent<Rigidbody>().isKinematic = true;
                }
                */

                
                if (!white) //Hack from existing system
                {
                    tempTargetPiece += 4;
                    currentTargetPiece = tempTargetPiece;
                }

                if (currentTargetPiece >= 10)
                {
                    Debug.Log("ERROR: PIECE INDEX IS 10");
                }

                #region AI Hand position Adjustment
                //piece 1
                if (currentTargetPiece == 5)
                {
                    if (pieces[currentTargetPiece] == 0) // starting point off board
                    {
                        lefthand = true;
                    }
                    else if (pieces[currentTargetPiece] >= 1 && pieces[currentTargetPiece] <= 8)
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
                else if (currentTargetPiece == 6)
                {
                    if (pieces[currentTargetPiece] == 0) // starting point off board
                    {
                        lefthand = true;
                    }
                    else if (pieces[currentTargetPiece] >= 1 && pieces[currentTargetPiece] <= 8)
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
                else if (currentTargetPiece == 7)
                {
                    if (pieces[currentTargetPiece] == 0) // starting point off board
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
                    else if (pieces[currentTargetPiece] >= 1 && pieces[currentTargetPiece] <= 8)
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
                else if (currentTargetPiece == 8)
                {
                    if (pieces[currentTargetPiece] == 0) // starting point off board
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
                    else if (pieces[currentTargetPiece] >= 1 && pieces[currentTargetPiece] <= 8)
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
                else if (currentTargetPiece == 9)
                {
                    if (pieces[currentTargetPiece] == 0) // starting point off board
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
                    else if (pieces[currentTargetPiece] >= 1 && pieces[currentTargetPiece] <= 8)
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

                //converting the AI piece's int info to game object
                //set ikTargetObj = piece pos
                if (lefthand)
                {
                    restingdestination = restingright.transform.position + RightIKOffset;// +wristoffset;
                    destination = boardpieces[currentTargetPiece].transform.position + LeftIKOffset;
                }
                else
                {
                    restingdestination = restingleft.transform.position + LeftIKOffset;
                    destination = boardpieces[currentTargetPiece].transform.position + RightIKOffset;
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

                #endregion
                lookdestination = destination;

                //Define the current turn's target piece GameObject based on the latest currentTargetPiece
                currentTargetPieceGameObject = boardpieces[currentTargetPiece].gameObject;
                currentPieceBehaviour = currentTargetPieceGameObject.GetComponent<PieceBehaviour>(); //and its PieceBehaviour component
                currentRigidBody = currentTargetPieceGameObject.GetComponent<Rigidbody>(); //and its RigidBody  

                Debug.Log("currentTargetPiece = " + currentTargetPiece);
                Debug.Log("AI: Going to move " + boardpieces[currentTargetPiece].gameObject.name);
                Debug.Log("AI PieceBehaviour: " + boardpieces[currentTargetPiece].gameObject.GetComponent<PieceBehaviour>().gameObject.name);
                Debug.Log("AI currentPieceBehaviour " + currentPieceBehaviour.gameObject.name);
                Debug.Log("AI: destination = " + turn.destination);
                //currentRigidBody.isKinematic = true;
                currentPieceBehaviour.AIMovePiece(turn.destination);
                state = AI_STATES.S_IKtoPIECEGRAB;
                break;
            #endregion

            #region statePieceMoving
            case AI_STATES.S_IKtoPIECEGRAB: //Move AI hand to the target piece
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

            #region PieceGrabbing
            case AI_STATES.S_PIECEGRABBING: //Set the target destination
                pieces[currentTargetPiece] = turn.destination; //update current target piece 'position' to the turn.destination value
                if (lefthand)
                {
                    destination = dropspots[turn.destination].transform.position + LeftIKOffset;
                }
                else
                {
                    destination = dropspots[turn.destination].transform.position + RightIKOffset;
                }
                lookdestination = destination;

                #region OldVersionPieceKickHandling
                //old system player piece kicking handling.
                //The new system (Zak) handle kicking on PieceBehaviour.cs and pass the info to this.PiecePositionCheck()
                /*
                for (int i = 0; i < 5; i++) //for board 0-4 (player pieces)
                {
                    if (pieces[i] == turn.destination) //if the player piece is on the AI destination, kick it
                    {
                        if (turn.destination > 4 && turn.destination < 13)
                        {
                            pieces[i] = 0;
                            boardpieces[i].transform.position = piecerespawn[i].transform.position;
                            Debug.Log("Opponent knocks player: board[" + i + "] = 0");
                            break;
                        }
                    }
                }
                */
                #endregion

                state = AI_STATES.S_IKtoDROP;
                break;
            #endregion

            case AI_STATES.S_IKtoDROP: //Moving the piece to the target destination
                //Debug.Log("AI: moving piece: " + currentPieceBehaviour.gameObject.name);
                
                if (lefthand)
				{
                    boardpieces[currentTargetPiece].transform.position = LeftHand.transform.position;
                    if ((ltarget.transform.position - destination).magnitude < lerpoffset)
					{
                        boardpieces[currentTargetPiece].transform.position = dropspots[turn.destination].transform.position;//boardpieces[aipiece].transform.position = destination - LeftIKOffset;
                        anim.SetBool("LeftGrab", false);
                        state = AI_STATES.S_PIECEDROPPING;
					}
				}
				else
				{
					boardpieces[currentTargetPiece].transform.position = RightHand.transform.position;
                    if ((rtarget.transform.position - destination).magnitude < lerpoffset)
                    {
                        boardpieces[currentTargetPiece].transform.position = dropspots[turn.destination].transform.position;//boardpieces[aipiece].transform.position = destination - RightIKOffset;
                        anim.SetBool("RightGrab", false);
                        state = AI_STATES.S_PIECEDROPPING;
					}
				}
				break;

            case AI_STATES.S_PIECEDROPPING: //drop the piece and move the AI hand back to rest (default) position
                //currentRigidBody.isKinematic = false;
                currentPieceBehaviour.AIDropPiece();
                /*
                foreach (GameObject p in boardpieces)
                {
                    p.GetComponent<Rigidbody>().isKinematic = false;
                }
                */
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
                /* handled on PhaseManager
                if (pieces[5] == 15 && pieces[6] == 15 && pieces[7] == 15 && pieces[8] == 15 && pieces[9] == 15)
                {
                    Debug.Log("AI Wins");
                    Time.timeScale = 0;
                }
                */
                lerplighton = false;
                lerplightoff = true;
                state = AI_STATES.S_IKtoWAIT;
                break;

            #endregion

            #region stateWait
            case AI_STATES.S_IKtoWAIT:   
				if (lefthand)
				{
                    //should be similar to PieceManager.PieceDropCheck()
					if ((ltarget.transform.position - destination).magnitude < lerpoffset)
					{                        
                        if (turn.destination == 4 || turn.destination == 8 || turn.destination == 14) //if turn.destination is rosette
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
                        if (turn.destination == 4 || turn.destination == 8 || turn.destination == 14) //if turn.destination is rosette
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
