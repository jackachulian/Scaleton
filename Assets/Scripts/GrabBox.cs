using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabBox : MonoBehaviour
{
    private List<GameObject> grabbableObjects;

    private GameObject grabbedBox;

    private Rigidbody2D playerRb;
    private Rigidbody2D boxRb;
    public Rigidbody2D BoxRb {get{return boxRb;}}

    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject grabPoint;

    [SerializeField] private Vector2 dropCheckOffset = new Vector2(-0.25f, 0);
    [SerializeField] private Vector2 dropCheckSize = new Vector2(0.5f, 0.5f);

    [SerializeField] private Vector2 forwardThrowForce = new Vector2(5f, 2f);

    [SerializeField] private LayerMask obstructionLayerMask;

    private int grabbedObjectLayer;

    private void Awake() {
        grabbableObjects = new List<GameObject>();
        playerRb = playerController.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.GetComponent("isGrabable") != null){
            Debug.Log("Grabbable object entered!");
            grabbableObjects.Add(c.gameObject);
        }
        else{
            Debug.Log("Non-grabbable object entered!");
        }
    }

    void OnTriggerExit2D(Collider2D c){
        bool removed = grabbableObjects.Remove(c.gameObject);
        if(removed){
            Debug.Log("Grabbable left range!");
        }
    }

    public void GrabPressed() {
        if (grabbedBox) {
            Drop();
        } 
        else if (grabbableObjects.Count > 0) {
            Grab(); 
        } 
        else {
            Debug.Log("Can't grab!");
        }
    }

    public void Grab(){
        // grab the closest gameobject to grab point
        grabbedBox = grabbableObjects.AsQueryable().OrderBy(obj => Vector2.Distance(obj.transform.position, grabPoint.transform.position)).First();

        boxRb = grabbedBox.GetComponent<Rigidbody2D>();
        boxRb.isKinematic = true;
        grabbedBox.GetComponent<Collider2D>().enabled = false;
        
        grabbedObjectLayer = grabbedBox.layer;
        grabbedBox.layer = LayerMask.NameToLayer("Grabbed");

        playerRb.mass += boxRb.mass;

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
            boxRb.isKinematic = false;
            grabbedBox.GetComponent<Collider2D>().enabled = true;

            grabbedBox.layer = grabbedObjectLayer;

            playerRb.mass -= boxRb.mass;

            grabbedBox.transform.parent = null;
            grabbedBox.transform.rotation = Quaternion.identity;

            boxRb.velocity = Vector2.zero;
            Vector2 throwForce = new Vector2(forwardThrowForce.x*playerController.FacingDirection, forwardThrowForce.y);
            boxRb.AddForce(throwForce, ForceMode2D.Impulse);

            // Conservation of momentum
            // Vector2 initialPlayerMomentum = playerRb.velocity * playerRb.mass;
            // Vector2 throwForce = new Vector2(forwardThrowForce.x * playerController.FacingDirection, forwardThrowForce.y);
            // boxRb.AddForce(throwForce, ForceMode2D.Impulse);

            // Vector2 boxMomentum = boxRb.velocity * boxRb.mass;
            // Vector2 playerMomentum = initialPlayerMomentum - boxMomentum;
            // Vector2 playerVelocity = playerMomentum / playerRb.mass;
            // playerRb.velocity = playerVelocity;

            grabbedBox = null;
            boxRb = null;
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
