using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{   
    [SerializeField] private float breakForce = 5f;

    [SerializeField] private GameObject breakShatterPrefab;

    void OnCollisionEnter2D(Collision2D c){
        if (!c.gameObject.GetComponent<Grabbable>()) return;

        Debug.Log("relvel: "+c.relativeVelocity + " - "+c.relativeVelocity.magnitude);
        Vector2 force = c.relativeVelocity * c.rigidbody.mass;
        if(force.magnitude > breakForce) {
            Break(force);
        }
    }

    void Break(Vector2 force) {
        GameObject breakShatter = Instantiate(breakShatterPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);

        Vector2 impulseForce = force.normalized * (force.magnitude - breakForce);
        
        foreach (Rigidbody2D rb in breakShatter.GetComponentsInChildren<Rigidbody2D>()) {
            rb.AddForce(impulseForce * 0.75f, ForceMode2D.Impulse);
            rb.AddForce(UnityEngine.Random.insideUnitCircle * 2f, ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-30f, 30f));
        }
    }
}
