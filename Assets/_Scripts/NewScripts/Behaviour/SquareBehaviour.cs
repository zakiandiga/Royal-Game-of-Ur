using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SquareBehaviour : MonoBehaviour
{
    [SerializeField] private SquareOwner squareOwner;
    public enum SquareOwner
    {
        Player,
        AI,
        Shared
    }

}
