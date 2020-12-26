using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour
{
    public Animator animator;
    public bool ikActive = false;

    public GameObject looktarget;
	public GameObject ltarget;
	public GameObject rtarget;

    public GameObject rightElbow;
    public GameObject rightHand;
    public GameObject leftElbow;
    public GameObject leftHand;

    public Transform rightHandObj = null;
    public Transform rightElbowObj = null;
    public Transform leftHandObj = null;
    public Transform leftElbowObj = null;

    public bool idlelerping;
    public float lerpp;
    public GameObject RightHand;
    private Vector3 IKOffset;

    void Start()
    {
        animator = GetComponent<Animator>();

        idlelerping = false;
        lerpp = 0;
        IKOffset = RightHand.transform.localPosition;
    }

    void Update()
	{

    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                if (idlelerping) // from walking to idle anim
                {
                    lerpp = Mathf.Lerp(lerpp, 1, Time.deltaTime/4);
                    animator.SetLookAtWeight(lerpp);
                    animator.SetLookAtPosition(looktarget.transform.position);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, lerpp);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lerpp);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rtarget.transform.position);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, ltarget.transform.position);
                    if (lerpp > 0.975f)
                    {
                        idlelerping = false;
                    }
                }
                else
                {
                    // Set the look target position, if one has been assigned
                    if (looktarget != null)
                    {
                        animator.SetLookAtWeight(1);// Mathf.Lerp(0, 1, Time.deltaTime));
                        animator.SetLookAtPosition(looktarget.transform.position);
                    }
                    else
                    {
                        animator.SetLookAtWeight(0);
                    }

                    // Set the right hand target position and rotation, if one has been assigned
                    if (rightHandObj != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        //animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);

                        animator.SetIKPosition(AvatarIKGoal.RightHand, rtarget.transform.position);
                        //animator.SetIKRotation(AvatarIKGoal.RightHand, rtarget.transform.rotation);
                        //animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowObj.position);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                        //animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                    }

                    // Set the left hand target position and rotation, if one has been assigned
                    if (leftHandObj != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        //animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);

                        animator.SetIKPosition(AvatarIKGoal.LeftHand, ltarget.transform.position);
                        //animator.SetIKRotation(AvatarIKGoal.RightHand, ltarget.transform.rotation);
                        //animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowObj.position);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                        //animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
                    }
                }
            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            /*else
            {
                animator.SetLookAtWeight(0);

                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            }*/
        }
    }
}