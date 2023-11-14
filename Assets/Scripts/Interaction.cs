// Allows theplayer to interact with nearby Interactables.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interaction : MonoBehaviour {
    private HashSet<Interactable> nearbyInteractables;

    private Interactable closestInteractable;

    private Interactable checkClosestInteractable;

    [SerializeField] private Transform interactPoint;

    [SerializeField] private Material defaultMaterial, outlineMaterial;

    [SerializeField] private PlayerController playerController;

    // Mainly to prevent picking up boxes from behind doors.
    [SerializeField] private LayerMask interactBlockMask;

    private void Awake() {
        nearbyInteractables = new HashSet<Interactable>();
    }

    private void FixedUpdate() {
        RefreshNearestInteractable();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        Interactable interactable = c.gameObject.GetComponent<Interactable>();
        if(interactable) {
            nearbyInteractables.Add(interactable);
        } 
    }

    void OnTriggerExit2D(Collider2D c)
    {
        Interactable interactable = c.gameObject.GetComponent<Interactable>();
        if (interactable) nearbyInteractables.Remove(interactable);
    }

    bool IsObstructed(Interactable interactable) {
        // if line from player to interactable is blocked by an obstacle, cannot be interacted with
        Debug.DrawLine(playerController.transform.position, interactable.transform.position, Color.magenta);
        var hit = Physics2D.Linecast(playerController.transform.position, interactable.transform.position, interactBlockMask);
        return hit.collider != null && hit.transform.gameObject != interactable.gameObject;
    }

    public void RefreshNearestInteractable(){
        // if player doesn't have control, they can't interact with anything.
        // if holding something, also can't interact with anything else.
        if (!playerController.HasControl() || playerController.GrabBox.IsHoldingBox()) {
            checkClosestInteractable = null;
        } 
        // otherwise check all nearby objects to update which is the closest
        else {
            // checkClosestInteractable = nearbyInteractables.AsQueryable()
            // .Where(obj => !IsObstructed(obj))
            // .OrderBy(obj => Vector2.Distance(obj.gameObject.transform.position, interactPoint.transform.position))
            // .FirstOrDefault();

            if (nearbyInteractables.Count == 0) {
                checkClosestInteractable = null;
            } else {
                float closestDistance = float.MaxValue;
                foreach (var obj in nearbyInteractables) {
                    if (IsObstructed(obj)) continue;
                    float distance = Vector2.Distance(obj.gameObject.transform.position, interactPoint.transform.position);
                    if (distance < closestDistance) {
                        checkClosestInteractable = obj;
                    }
                }
            }
        }

        if (checkClosestInteractable != closestInteractable) 
        {
            // unhover previous closest if it exists
            if (closestInteractable) 
            {
                if (closestInteractable.SpriteRenderer) closestInteractable.SpriteRenderer.material = defaultMaterial;
                closestInteractable.Unhover();
            }

            closestInteractable = checkClosestInteractable;

            // hover new closest if it exists
            if (closestInteractable) 
            {
                if (closestInteractable.SpriteRenderer) closestInteractable.SpriteRenderer.material = outlineMaterial;
                closestInteractable.Hover();
            }
        }
    }

    // bool CanBeInteracted(Interactable interactable) {
    //     if (grabBox.IsHoldingBox() && interactable.IsGrabbable) return false;
    //     return true;
    // }

    public void InteractNearest() {
        if (playerController.GrabBox.IsHoldingBox()) {
            playerController.GrabBox.ReleaseGrabbed(true);
        }
        else if (closestInteractable) {
            if (GrabNearestIfPossible()) return;
            closestInteractable.Interact();
        }
    }

    public bool CancelNearest() {
        if (playerController.GrabBox.IsHoldingBox()) {
            playerController.GrabBox.ReleaseGrabbed(false);
            return true;
        }
        else if (closestInteractable) {
            if (GrabNearestIfPossible()) return true;
            closestInteractable.Cancel();
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if the nearest interactable was a box and was grabbed</returns>
    bool GrabNearestIfPossible() {
        if (!playerController.GrabBox.IsHoldingBox() && closestInteractable.IsGrabbable) {
            Grabbable grabbable = closestInteractable.GetComponent<Grabbable>();
            playerController.GrabBox.Grab(grabbable);
            return true;
        }

        return false;
    }
}