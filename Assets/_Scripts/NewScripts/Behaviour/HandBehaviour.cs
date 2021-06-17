using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandBehaviour : MonoBehaviour
{
    private SphereCollider interactorCollider;

    // Start is called before the first frame update
    void Start()
    {
        interactorCollider = GetComponent<SphereCollider>();    
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
