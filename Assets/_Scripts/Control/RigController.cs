using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RigController : MonoBehaviour
{
    [SerializeField] private InputActionReference movement;

    // Start is called before the first frame update
    void Start()
    {
        movement.action.Enable();   
    }

    private void OnDestroy()
    {
        movement.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
