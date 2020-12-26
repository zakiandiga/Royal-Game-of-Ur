using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingtoIdleTrigger : MonoBehaviour {

    public GameObject Opponent;
    public IKControl ik;
    public Transform rhandobj;
    public Transform lhandobj;
    public GameObject lookobj;
    private Animator anim;
    public GameObject trigger;

    public bool lerping;

    public GameObject GameManager;
    public PlayerStateMachine playerstate;

    public AudioSource step;

    // Use this for initialization
    void Start () {
        Opponent = this.gameObject;
        ik = Opponent.GetComponent<IKControl>();

        anim = this.GetComponent<Animator>();
        anim.SetBool("Idle", false);

        lerping = false;

        playerstate = GameManager.GetComponent<PlayerStateMachine>();

        step = this.GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        if (lerping)
        {
            if (ik.idlelerping == false)
            {
                lerping = false;
                this.GetComponent<WalkingtoIdleTrigger>().enabled = false; //kill script
            }
        }
		else if (this.transform.position.x > trigger.transform.position.x)
        {
            anim.SetBool("Idle", true);
            playerstate.state = PlayerStateMachine.PLAYER_STATES.S_WAITING;
            lerping = true;
            ik.idlelerping = true;
            ik.rightHandObj = rhandobj;
            ik.leftHandObj = lhandobj;
            ik.looktarget = lookobj;
        }
        else
        {
            ik.rightHandObj = ik.leftHandObj = null;
            ik.looktarget = null;
        }
	}

    void StepSound()
    {
        step.Play();
    }
}
