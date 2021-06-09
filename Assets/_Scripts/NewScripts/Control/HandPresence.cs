using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class HandPresence : MonoBehaviour
{
    #region XR_Component
    public InputDeviceCharacteristics controllerCharacteristics;
    public List<GameObject> controllerPrefabs;
    public GameObject handModelPrefab;
    private GameObject spawnedController;
    private GameObject spawnedHandModel;
    private InputDevice targetDevice;
    #endregion

    #region AdditionalComponent
    private Animator handAnimator;


    #endregion

    #region InputValues
    private float triggerValue, gripValue = 0;
    private Vector2 axisValue;
    #endregion

    #region PlayerStates
    private HandState handState = HandState.Idle;
    public enum HandState
    {
        IsPinching,
        IsGripping,
        Idle
    }
    #endregion

    #region ControllerState
    private ControllerState controllerState = ControllerState.Hand;
    public enum ControllerState
    {
        Controller,
        Hand
    }
    #endregion

    #region Events
    //hand idle
    //is pinching
    //is grabbing
    public static event Action<HandPresence> OnExitPinch;
    public static event Action<HandPresence> OnEnterPinch;
    public static event Action<HandPresence> OnExitGrab;
    public static event Action<HandPresence> OnEnterGrip;
    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    private void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            GameObject prefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name);
            if (prefab)
            {
                spawnedController = Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogError("Cannot find controller model");
                spawnedController = Instantiate(controllerPrefabs[0], transform);
            }

            spawnedHandModel = Instantiate(handModelPrefab, transform);
            handAnimator = spawnedHandModel.GetComponent<Animator>();
        }

        ControllerDisplaySwitch();
    }

    private void HandGestureSwitch() //This function might not needed
    {
        switch (handState)
        {
            case HandState.IsPinching:
                //Debug.Log("Announce, this hand is pinching");
                //what else?
                break;
            case HandState.IsGripping:
                //Debug.Log("Announce, this hand is grabbing");
                //what else?
                break;
            case HandState.Idle:
                //Debug.Log("Announce, this hand is idle");
                //what else?
                break;
        }
    }

    private void ControllerDisplaySwitch() //Call this method on TryInitialize() and if needed somewhere else, to switch between hand/controller display
    {
        if (targetDevice.isValid && controllerState == ControllerState.Controller)
        {
            spawnedHandModel.SetActive(false);
            spawnedController.SetActive(true);

        }
        else if (targetDevice.isValid && controllerState == ControllerState.Hand)
        {
            spawnedHandModel.SetActive(true);
            spawnedController.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!targetDevice.isValid) //repeat this until device initialized
        {
            TryInitialize();
        }

        #region InputValue
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float trigger))
        {
            triggerValue = trigger;
            handAnimator.SetFloat("Trigger", triggerValue);
        }

        if(targetDevice.TryGetFeatureValue(CommonUsages.grip, out float grip))
        {
            gripValue = grip;
            handAnimator.SetFloat("Grip", gripValue);
        }

        #endregion

        #region MovementState
        if(triggerValue <= 0.1f && gripValue <= 0.1f)
        {
            if(handState != HandState.Idle)
            {
                if (handState == HandState.IsGripping) //Exit from grabbing announcer
                {
                    OnExitGrab?.Invoke(this);
                    //Debug.Log(this.gameObject.name + " exits Grabbing");
                }
                if(handState == HandState.IsPinching) //Exit from pinching announcer
                {
                    OnExitPinch?.Invoke(this);
                }
                handState = HandState.Idle;

                HandGestureSwitch();
            }            
        }

        else if(triggerValue >= 0.9f && gripValue <= 0.1f) //0.9f because TRUE value is not 1
        {
            if(handState != HandState.IsPinching)
            {
                if (handState == HandState.IsGripping) //Exit from grabbing
                {
                    OnExitGrab?.Invoke(this);
                    //Debug.Log(this.gameObject.name + " exits Grabbing");
                }
                handState = HandState.IsPinching;

                HandGestureSwitch();
                OnEnterPinch?.Invoke(this); //Enter pinch announcer
            }            
        }

        else if(gripValue >= 0.9f)
        {
            if(handState != HandState.IsGripping)
            {
                if (handState == HandState.IsPinching) //Exit from pinching announcer
                {
                    OnExitPinch?.Invoke(this);
                }
                handState = HandState.IsGripping;

                HandGestureSwitch();
                OnEnterGrip?.Invoke(this);  //Enter grab announcer
                //Debug.Log(this.gameObject.name + " enter Grabbing");
            }
        }

        #endregion
    }
}