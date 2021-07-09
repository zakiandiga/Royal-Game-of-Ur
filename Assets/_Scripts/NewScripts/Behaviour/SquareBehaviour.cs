﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SquareBehaviour : MonoBehaviour
{
    [SerializeField] private Transform squareChecker;
    [SerializeField] private float checkerRadius;
    [SerializeField] private LayerMask checkerLayerMask;

    [SerializeField] private SquareOwner squareOwner;

    private Collider boxCollider;

    public bool isRosette;
    public bool isFinish;


    private void Start()
    {
        boxCollider = gameObject.GetComponent<Collider>();
    }

    public enum SquareOwner
    {
        White,
        Black,
        Shared
    }

    public SquareTenant squareTenant;
    public enum SquareTenant
    {
        Empty,
        White,
        Black
    }

    public void CheckTenant()
    {
        float range = 10f;
        float raycastOffset = -2f; //Y offset of the raycast origin
        Vector3 raycastOrigin = new Vector3(transform.position.x, transform.position.y + raycastOffset, transform.position.z);
        RaycastHit hit;
        Ray ray = new Ray(raycastOrigin, Vector3.up);      
        
        if (Physics.Raycast(ray, out hit, range, checkerLayerMask))
        {
            if(hit.collider.gameObject.layer == 31) //31 = PlayerPiece layer
            {
                //Debug.Log(this.gameObject.name + " square occupied by a WHITE piece");
                squareTenant = SquareTenant.White;
            }                

            if(hit.collider.gameObject.layer == 30) //30 = AI piece layer
            {
                //Debug.Log(this.gameObject.name + " square occupied by a BLACK piece");
                squareTenant = SquareTenant.Black;
            }
                              
        }        
        else
        {
            //Debug.Log(this.gameObject.name + " square is empty");
            squareTenant = SquareTenant.Empty;
        }            
    }

}
