using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBehaviour : MonoBehaviour
{
    MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        //PieceBehaviour.OnRaycastHit += LightSwitch;
    }

    /*
    private void LightSwitch(string name)
    {
        if (name == this.gameObject.name && meshRenderer.enabled == false)
        {
            meshRenderer.enabled = true;
        }
        if (name != this.gameObject.name && meshRenderer.enabled == true)
        {
            meshRenderer.enabled = false;
        }
    }
    */

    // Update is called once per frame
    void Update()
    {
        
    }
}
