using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class BreakableBlock : MonoBehaviour
{   
    [SerializeField] private float breakForce = 5f;

    [SerializeField] private GameObject breakShatterPrefab;

    void OnCollisionEnter2D(Collision2D c){
        if (!c.gameObject.GetComponent<Grabbable>()) return;

        Debug.Log("relvel: "+c.relativeVelocity + " - "+c.relativeVelocity.magnitude);
        Vector2 force = c.relativeVelocity * c.rigidbody.mass;
        Debug.Log("force: "+force+" (magnitude: "+force.magnitude+")");
        float centerForce = Vector2.Dot(force, transform.position - c.transform.position);
        Debug.Log("center force: "+centerForce);

        if(centerForce > breakForce) {
            Break(force);
        }
    }

    void Break(Vector2 force) {
        if (!breakShatterPrefab) {
            Destroy(gameObject); 
            return;
        }
        
        GameObject breakShatter = Instantiate(breakShatterPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);

        Vector2 impulseForce = force.normalized * (force.magnitude - breakForce);
        
        foreach (Rigidbody2D rb in breakShatter.GetComponentsInChildren<Rigidbody2D>()) {
            rb.AddForce(impulseForce * 0.5f * rb.mass, ForceMode2D.Impulse);
            rb.AddForce(UnityEngine.Random.insideUnitCircle * 2f * rb.mass, ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-8f, 8f) * rb.mass);
            Destroy(rb.gameObject, 30f);
        }
    }
}
