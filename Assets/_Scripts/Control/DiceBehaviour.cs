using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceBehaviour : MonoBehaviour
{
    private XRGrabInteractable grabControl;
    private Rigidbody rb;
    [SerializeField] private Transform spawner;

    #region DicetypeIdentifier
    [SerializeField] private DiceType diceType = DiceType.Unassigned;
    private enum DiceType
    {
        Number,
        Bool,
        Unassigned,
    }
    #endregion

    #region SideIdentifier
    public Transform side1, side2, side3, side4;
    public float checkRadius;
    public LayerMask altarMask;
    #endregion

    #region DiceState
    private bool interactable = true; //modified from observer

    private DiceState diceState = DiceState.Idle;
    public enum DiceState
    {
        Idle,
        Grabable,
        OnHand,
        Thrown
    }
    #endregion

    #region ResultAnnouncerEvents
    public static event Action<int> OnDiceNumberResult;
    public static event Action<int> OnDiceBoolResult;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabControl = GetComponent<XRGrabInteractable>();
        grabControl.enabled = false;
    }    

    private void OnEnable()
    {
        HandPresence.OnEnterGrab += DiceGrab;
        HandPresence.OnExitGrab += DiceThrow;
        PhaseManager.OnEnterDiceRoll += InteractableOn;
        PhaseManager.OnExitDiceRoll += InteractableOff;
    }

    private void OnDisable()
    {
        HandPresence.OnExitGrab -= DiceThrow;
        HandPresence.OnEnterGrab -= DiceGrab;
        PhaseManager.OnEnterDiceRoll -= InteractableOn;
        PhaseManager.OnExitDiceRoll -= InteractableOff;
    }

    #region DiceInteractableSwitch
    private void InteractableOn(PhaseManager phase)
    {
        if(!interactable)
            interactable = true;
    }

    private void InteractableOff(PhaseManager phase) //is this redundant with DiceCheck()?
    {
        if(interactable)
            interactable = false;
    }
    #endregion

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Hand" && interactable)
        {
            if (diceState == DiceState.Idle)
            {
                diceState = DiceState.Grabable;
                grabControl.enabled = true;
                Debug.Log(this.gameObject.name + " is GRABABLE");
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Hand" && interactable)
        {
            if (diceState == DiceState.Grabable)
            {
                diceState = DiceState.Idle;
                grabControl.enabled = false;
                Debug.Log(this.gameObject.name + " is NOT GRABABLE");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)  //Out of altar throw handler
    {
        if (collision.collider.tag != "altar")
        {
            transform.position = spawner.position;
        }
    }

    private void DiceGrab(HandPresence hand) //On dice grabbed
    {
        if(diceState == DiceState.Grabable)
        {
            diceState = DiceState.OnHand;
            Debug.Log(this.gameObject.name + " ON HAND");
        }        
    }

    private void DiceThrow(HandPresence hand) //on dice thrown
    {
        if(diceState == DiceState.OnHand)
        {
            diceState = DiceState.Thrown;
            grabControl.enabled = false;
            Debug.Log(this.gameObject.name + " THROWN");
            //polishing checklist
            //add force to dice
        }
    }

    private void DiceCheck() //Refactor this if needed
    {
        int rollResult;
        bool one, two, three, four;

        one = Physics.CheckSphere(side1.position, checkRadius, altarMask);
        two = Physics.CheckSphere(side2.position, checkRadius, altarMask);
        three = Physics.CheckSphere(side3.position, checkRadius, altarMask);
        four = Physics.CheckSphere(side4.position, checkRadius, altarMask);

        if(diceType == DiceType.Number)
        {
            if (one)
                rollResult = 1;
            else if (two)
                rollResult = 2;
            else if (three)
                rollResult = 3;
            else if (four)
                rollResult = 4;
            else
            {
                rollResult = -1;
                Debug.Log("Dice result error");
            }

            Debug.Log(this.gameObject.name + " result is " + rollResult);
            interactable = false;
            OnDiceNumberResult?.Invoke(rollResult);
        }
        else if (diceType == DiceType.Bool)
        {
            if (one)
                rollResult = 0;
            else if (two)
                rollResult = 0;
            else if (three)
                rollResult = 1;
            else if (four)
                rollResult = 1;
            else
            {
                rollResult = -1;
                Debug.Log("Dice result error");
            }

            Debug.Log(this.gameObject.name + " result is " + rollResult);
            interactable = false;
            OnDiceBoolResult?.Invoke(rollResult);
        }
        else
        {
            Debug.Log("Dice roll error, Dice Type not assigned");            
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        switch (diceState)
        {
            case DiceState.Idle:
                //Things to Update() during idle
                break;
            case DiceState.Grabable:
                //Things to Update() during grabable
                break;
            case DiceState.OnHand:
                //Things to Update() during OnHand
                //run physic based on hand movement
                //Keep track on velocity
                break;
            case DiceState.Thrown:
                //bool CheckResult() until return true
                //if return true, switch to DiceState.Idle
                if(rb.velocity.magnitude <= 0.0001f)
                {
                    diceState = DiceState.Idle;
                    DiceCheck();
                }
                break;

        }
            
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(side1.position, checkRadius);
        Gizmos.DrawWireSphere(side2.position, checkRadius);
        Gizmos.DrawWireSphere(side3.position, checkRadius);
        Gizmos.DrawWireSphere(side4.position, checkRadius);
    }
}
