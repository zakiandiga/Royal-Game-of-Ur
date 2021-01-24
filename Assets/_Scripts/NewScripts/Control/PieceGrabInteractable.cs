using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PieceGrabInteractable : XRGrabInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnActivate(XRBaseInteractor interactor, GameObject piece)
    {
        OnSelectEntered(interactor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
