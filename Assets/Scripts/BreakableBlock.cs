using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] Collider2D collider; 
    
void OnCollisionEnter2D(Collision2D c){
        if(Math.Abs(c.rigidbody.velocity.x) > 1 || Math.Abs(c.rigidbody.velocity.y) > 1){
            if(c.gameObject.GetComponent<Grabbable>() == true){
                if(!c.gameObject.GetComponent<Grabbable>().isPlayer)
                Destroy(gameObject);
            }
        }
    }
}
