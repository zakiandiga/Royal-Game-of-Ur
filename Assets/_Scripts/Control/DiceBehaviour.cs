﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceBehaviour : MonoBehaviour
{
    //Set dice layers to dice (Dice should not collide with other dices)
    private XRGrabInteractable grabControl;
    private Rigidbody rb;
    [SerializeField] private Transform spawner;

    private float diceFallTreshold = 0.5f;
    private float resultDelay = 1.2f;
    private int rollResult;
    private SphereCollider grabCollider;

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
    [SerializeField] private bool interactable; //modify from observer

    [SerializeField] private DiceState diceState = DiceState.Idle;
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
        grabCollider = GetComponentInChildren<SphereCollider>();
        //grabCollider.SetActive(false); //uncomment this when the game loop/phase complete
        Debug.Log("Starting dice state = " + diceState);
  
    }    

    private void OnEnable()
    {
        HandPresence.OnEnterGrip += DiceGrab;
        HandPresence.OnExitGrab += DiceThrow;
        PhaseManager.OnEnterDiceRoll += InteractableOn;
        //PhaseManager.OnExitDiceRoll += InteractableOff;
    }

    private void OnDisable()
    {
        HandPresence.OnExitGrab -= DiceThrow;
        HandPresence.OnEnterGrip -= DiceGrab;
        PhaseManager.OnEnterDiceRoll -= InteractableOn;
        //PhaseManager.OnExitDiceRoll -= InteractableOff;
    }

    #region DiceInteractableSwitch
    private void InteractableOn(PhaseManager phase)
    {
        if(!interactable)
        {
            interactable = true;
            grabCollider.enabled = true;
        }
            
    }

    /*
    private void InteractableOff(int result) //is this redundant with DiceCheck()?
    {
        if(interactable)
        {
            interactable = false;
            grabCollider.SetActive(false);
        }
            

    }
    */
    #endregion

    public void GrabColliderEnter()
    {
        if (diceState == DiceState.Idle)
        {
            diceState = DiceState.Grabable;            
            //Debug.Log(this.gameObject.name + " is GRABABLE");
        }

    }

    public void GrabColliderExit()
    {
        if (diceState == DiceState.Grabable)
        {
            diceState = DiceState.Idle;            
            //Debug.Log(this.gameObject.name + " is NOT GRABABLE");
        }

    }


    private void DiceGrab(HandPresence hand) //On dice grabbed
    {
        if(diceState == DiceState.Grabable && interactable)
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
            Debug.Log("Current dice state = " + diceState);
            //polishing checklist
            //add force to dice
        }
    }

    private void DiceCheck() //Refactor this if needed
    {        
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

            Debug.Log(this.gameObject.name + " is thrown");
            grabCollider.enabled = false;
            //StartCoroutine(NumberResultDelay());
            OnDiceNumberResult?.Invoke(rollResult);
            transform.position = spawner.position;

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

            Debug.Log(this.gameObject.name + " is thrown");
            grabCollider.enabled = false;
            //StartCoroutine(BoolResultDelay());
            OnDiceBoolResult?.Invoke(rollResult);
            transform.position = spawner.position;

        }
        else
        {
            Debug.Log("Dice roll error, Dice Type not assigned");            
        }        
    }

    private IEnumerator NumberResultDelay()
    {

        yield return new WaitForSeconds(resultDelay);
        OnDiceNumberResult?.Invoke(rollResult);
        transform.position = spawner.position;

    }

    private IEnumerator BoolResultDelay()
    {

        yield return new WaitForSeconds(resultDelay);

        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y <= diceFallTreshold) //Out of place handler
        {
            transform.position = spawner.position;
        }

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
                    Debug.Log("Current Dice State = " + diceState + ", " + this.gameObject.name + " collider disabled");
                    this.DiceCheck();
                    if (interactable)
                    {
                        this.interactable = false;
                        this.grabCollider.enabled = false;
                    }
                    this.diceState = DiceState.Idle;
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
