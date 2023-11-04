using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabBox : MonoBehaviour
{
    private GameObject toGrab;
    public GameObject grabbedBox;
    void OnTriggerEnter2D(Collision2D c){
        Debug.Log("SOMETHING ENTERED");
        if(toGrab != null && grabbedBox == null){
            if(c.gameObject.GetComponent("isGrabbable") != null){
                Debug.Log("Grabbable object entered!");
                toGrab = c.gameObject;
            }
            else{
                Debug.Log("Non-grabbable object entered!");
            }
        }
        else{
            Debug.Log("An object entered, but you are already grabbing!");
        }
    }

    private void OnCollisionExit2D(Collision2D c){
        if(c.gameObject == toGrab){
            toGrab = null;
        }
    }

    public bool CanGrab(){
        return(toGrab != null);
    }
    public void Grab(){
        if(CanGrab()){
            grabbedBox = toGrab;
            toGrab = null;
            grabbedBox.transform.parent = transform;
            Debug.Log("Grab!");
        }
        else{
            Debug.Log("Can't grab!");
        }
    }
}
