using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_DiceBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private Transform spawner;

    private float diceFallTreshold = 0.5f;
    private float resultDelay = 1.2f;
    private int rollResult;

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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
