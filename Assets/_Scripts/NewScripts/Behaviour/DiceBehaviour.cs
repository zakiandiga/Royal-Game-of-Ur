using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceBehaviour : MonoBehaviour
{
    //Set dice layers to dice (Dice should not collide with other dices)
    private XRGrabInteractable grabControl;
    private Rigidbody rb;
    private ParticleSystem particle;
    [SerializeField] private Transform spawner;

    [SerializeField] private LayerMask interactableOn;
    [SerializeField] private LayerMask interactableOff;
    [SerializeField] private LayerMask pieceLayer;

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

    [SerializeField] private DiceState diceState = DiceState.Ready;
    public enum DiceState
    {
        Ready,
        Grabable,
        OnHand,
        Thrown,
        Waiting,
        AI_Throw
    }
    #endregion

    [SerializeField] private bool aIDice;
    private bool checkingAIDiceResult;

    #region ResultAnnouncerEvents
    public static event Action<int, bool> OnDiceNumberResult;
    public static event Action<int, bool> OnDiceBoolResult;
    public static event Action<GameObject, string> OnDiceStateChange;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();        
        grabControl = GetComponent<XRGrabInteractable>();
        grabCollider = GetComponentInChildren<SphereCollider>();
        particle = GetComponentInChildren<ParticleSystem>();

        Physics.IgnoreLayerCollision(this.gameObject.layer, pieceLayer, true);

        //grabCollider.SetActive(false); //uncomment this when the game loop/phase complete
        Debug.Log("Starting dice state = " + diceState);
         
    }    

    private void OnEnable()
    {
        if(!aIDice)
        {
            HandPresence.OnEnterGrip += DiceGrab;
            HandPresence.OnExitGrab += DiceThrow;
            PhaseManager.OnEnterDiceRoll += ReadyingDice;
        }

        if(aIDice)
        {
            AIAnimationStateMachine.OnDiceThrownAI += DiceThrowAI;
        }
    }

    private void OnDisable()
    {
        if(!aIDice)
        {
            HandPresence.OnExitGrab -= DiceThrow;
            HandPresence.OnEnterGrip -= DiceGrab;
            PhaseManager.OnEnterDiceRoll -= ReadyingDice;
        }
        if(aIDice)
        {
            AIAnimationStateMachine.OnDiceThrownAI -= DiceThrowAI;
        }
    }

#region PlayerDiceInteractableSwitch
    private void ReadyingDice(PhaseManager phase)
    {
        diceState = DiceState.Ready;        
        OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI      
    }
#endregion

#region PlayerDiceInteractor
    public void GrabColliderEnter()
    {
        if (diceState == DiceState.Ready)
        {
            diceState = DiceState.Grabable;
            OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI
            //Debug.Log(this.gameObject.name + " is GRABABLE");
        }

    }

    public void GrabColliderExit()
    {
        if (diceState == DiceState.Grabable)
        {
            diceState = DiceState.Ready;
            OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI
            //Debug.Log(this.gameObject.name + " is NOT GRABABLE");
        }

    }

    
    private void DiceGrab(HandPresence hand) //On dice grabbed
    {
        if(diceState == DiceState.Grabable) // && interactable)
        {
            diceState = DiceState.OnHand;

            OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI
            Debug.Log(this.gameObject.name + " ON HAND");
        }        
    }

    private void DiceThrow(HandPresence hand) //on dice thrown
    {
        if(diceState == DiceState.OnHand)
        {
            diceState = DiceState.Thrown;
            OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI
            Debug.Log("Current dice state = " + diceState);
            //polishing checklist
            //add force to dice
        }
    }
#endregion

    public void DiceThrowAI(string ai)
    {
        if(diceState != DiceState.AI_Throw)
        {
            diceState = DiceState.AI_Throw;
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
                Debug.Log(this.gameObject.name + " DiceCheck error");
            }

            //Debug.Log(this.gameObject.name + " is thrown");
            //grabCollider.enabled = false;
            //StartCoroutine(NumberResultDelay());
            OnDiceNumberResult?.Invoke(rollResult, aIDice);
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
                Debug.Log(this.gameObject.name + " DiceCheck error");
            }

            Debug.Log(this.gameObject.name + " is thrown");
            
            //StartCoroutine(BoolResultDelay());
            OnDiceBoolResult?.Invoke(rollResult, aIDice);
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
        OnDiceNumberResult?.Invoke(rollResult, aIDice);
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
            case DiceState.Waiting:
                //if (grabCollider.enabled)
                //    grabCollider.enabled = false;
                if (grabControl.interactionLayerMask != interactableOff && !aIDice)
                {
                    grabControl.interactionLayerMask = interactableOff;
                }

                if (particle.isEmitting)
                {                    
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                break;
            case DiceState.Ready:
                //Things to Update() during idle
                if(grabControl.interactionLayerMask != interactableOn && !aIDice)
                {
                    grabControl.interactionLayerMask = interactableOn;
                }
                if(!particle.isEmitting && !aIDice)
                {                    
                    particle.Play();
                }                
                break;
            case DiceState.Grabable:
                //Things to Update() during grabable
                if (!particle.isEmitting && !aIDice)
                {                    
                    particle.Play();
                }
                break;
            case DiceState.OnHand:
                //Things to Update() during OnHand
                //run physic based on hand movement
                //Keep track on velocity
                if(particle.isEmitting)
                {                    
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                break;
            case DiceState.Thrown:
                //bool CheckResult() until return true
                //if return true, switch to DiceState.Idle
                if (grabControl.interactionLayerMask != interactableOff && !aIDice)
                {
                    grabControl.interactionLayerMask = interactableOff;
                }
                if (particle.isEmitting)
                {                    
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                if (rb.velocity.magnitude <= 0.0001f)
                {                    
                    //Debug.Log("Current Dice State = " + diceState + ", " + this.gameObject.name + " collider disabled");

                    this.DiceCheck();

                    //this.grabCollider.enabled = false;
                    this.diceState = DiceState.Waiting;

                    OnDiceStateChange?.Invoke(this.gameObject, this.diceState.ToString()); //Debug UI
                }
                break;
            case DiceState.AI_Throw:
                if (rb.velocity.magnitude <= 0.0001f)
                {
                    //Debug.Log(this.gameObject.name + " stop falling, now DiceCheck()");
                    this.DiceCheck();
                    
                    this.diceState = DiceState.Waiting;
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
