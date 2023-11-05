using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabBox : MonoBehaviour
{
    private GameObject toGrab;
    private GameObject grabbedBox;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject grabPoint;

    [SerializeField] private Vector2 dropCheckOffset = new Vector2(-0.25f, 0);
    [SerializeField] private Vector2 dropCheckSize = new Vector2(0.5f, 0.5f);

    [SerializeField] private Vector2 forwardThrowForce = new Vector2(5f, 2f);

    [SerializeField] private LayerMask obstructionLayerMask;

    private int grabbedObjectLayer;

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

    public void GrabPressed() {
        if (toGrab != null) {
            Grab();
        } else if (grabbedBox != null) {
            Drop();
        } else {
            Debug.Log("Can't grab!");
        }
    }

    public void Grab(){
        grabbedBox = toGrab;
        toGrab = null;
        grabbedBox.GetComponent<Rigidbody2D>().isKinematic = true;
        grabbedBox.GetComponent<Collider2D>().enabled = false;
        
        grabbedObjectLayer = grabbedBox.layer;
        grabbedBox.layer = LayerMask.NameToLayer("Grabbed");

        grabbedBox.transform.parent = grabPoint.transform;
        grabbedBox.transform.localPosition = Vector2.zero;
        grabbedBox.transform.localRotation = Quaternion.identity;
        Debug.Log("Grab!");
    }

    // Attempt to let go of the box.
    // If grab point is obstructed by terrain, cannot drop here
    public void Drop() {
        Collider2D obstruction = Physics2D.OverlapBox(grabPoint.transform.position + (Vector3)dropCheckOffset*playerController.FacingDirection, dropCheckSize, 0f, obstructionLayerMask);

        Debug.Log(obstruction);
        if (obstruction) Debug.Log(LayerMask.LayerToName(obstruction.gameObject.layer));

        if (obstruction == null) {
            grabbedBox.GetComponent<Rigidbody2D>().isKinematic = false;
            grabbedBox.GetComponent<Collider2D>().enabled = true;

            grabbedBox.layer = grabbedObjectLayer;

            grabbedBox.transform.parent = null;
            grabbedBox.transform.rotation = Quaternion.identity;
            
            // Conservation of momentum
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            Rigidbody2D boxRb = grabbedBox.GetComponent<Rigidbody2D>();
            Vector2 initialPlayerMomentum = playerRb.velocity * playerRb.mass;

            boxRb.velocity = playerRb.velocity;
            Vector2 throwForce = new Vector2(forwardThrowForce.x * playerController.FacingDirection, forwardThrowForce.y);
            boxRb.AddForce(throwForce, ForceMode2D.Impulse);

            Vector2 boxMomentum = boxRb.velocity * boxRb.mass;
            Vector2 playerMomentum = initialPlayerMomentum - boxMomentum;
            Vector2 playerVelocity = playerMomentum / playerRb.mass;
            playerRb.velocity = playerVelocity;

            grabbedBox = null;
            Debug.Log("Box dropped");
        } else {
            Debug.Log("Can't drop here!");
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(grabPoint.transform.position + (Vector3)dropCheckOffset*playerController.FacingDirection, dropCheckSize);
    }
}
