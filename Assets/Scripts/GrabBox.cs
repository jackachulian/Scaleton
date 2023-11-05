using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabBox : MonoBehaviour
{
    private GameObject toGrab;
    public GameObject grabbedBox;
    public GameObject grabPoint;
    void OnTriggerEnter2D(Collider2D c){
        Debug.Log("SOMETHING ENTERED");
        if(toGrab == null && grabbedBox == null){
            if(c.gameObject.GetComponent("isGrabable") != null){
                Debug.Log("Grabbable object entered!");
                toGrab = c.gameObject;
            }
            else{
                Debug.Log("Non-grabbable object entered!");
            }
        }
        else{
            if(grabbedBox != null){
                Debug.Log("An object entered, but you are already grabbing!");
            }
            if (toGrab != null){
                Debug.Log("An object entered, but one is already in range!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D c){
        if(c.gameObject == toGrab){
            Debug.Log("Grabbable left range!");
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
            grabbedBox.GetComponent<Rigidbody2D>().isKinematic = true;
            grabbedBox.transform.parent = transform;
            Debug.Log("Grab!");
        }
        else{
            Debug.Log("Can't grab!");
        }
    }

    public void Update(){
        if(grabbedBox != null){
            grabbedBox.transform.position = Vector2.MoveTowards(grabPoint.transform.position,grabPoint.transform.position,Time.deltaTime*100);
        }
    }

}
