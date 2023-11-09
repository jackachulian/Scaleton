using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{   
    [SerializeField] private float breakForce = 5f;

    void OnCollisionEnter2D(Collision2D c){
        if (!c.gameObject.GetComponent<Grabbable>()) return;

        Debug.Log("relvel: "+c.relativeVelocity + " - "+c.relativeVelocity.magnitude);
        if(c.relativeVelocity.magnitude > breakForce) Destroy(gameObject);
    }
}
