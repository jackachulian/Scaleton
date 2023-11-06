// Allows theplayer to interact with nearby Interactables.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interaction : MonoBehaviour {
    private List<Interactable> nearbyInteractables;

    private Interactable closestInteractable;

    [SerializeField] private Transform interactPoint;

    [SerializeField] private Material defaultMaterial, outlineMaterial;

    [SerializeField] private GrabBox grabBox;

    private void Awake() {
        nearbyInteractables = new List<Interactable>();
    }

    private void FixedUpdate() {
        RefreshNearestInteractable();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        Interactable interactable = c.gameObject.GetComponent<Interactable>();
        if(interactable) nearbyInteractables.Add(interactable);
    }

    void OnTriggerExit2D(Collider2D c)
    {
        Interactable interactable = c.gameObject.GetComponent<Interactable>();
        if (interactable) nearbyInteractables.Remove(interactable);
    }

    void RefreshNearestInteractable(){
        Interactable checkClosestInteractable = nearbyInteractables.AsQueryable()
        .Where(obj => CanBeInteracted(obj))
        .OrderBy(obj => Vector2.Distance(obj.gameObject.transform.position, interactPoint.transform.position))
        .FirstOrDefault();

        if (checkClosestInteractable != closestInteractable) 
        {
            if (closestInteractable) 
            {
                if (closestInteractable.SpriteRenderer) closestInteractable.SpriteRenderer.material = defaultMaterial;
                closestInteractable.Unhover();
            }

            closestInteractable = checkClosestInteractable;

            if (closestInteractable) 
            {
                if (closestInteractable.SpriteRenderer) closestInteractable.SpriteRenderer.material = outlineMaterial;
                closestInteractable.Hover();
            }
        }
    }

    bool CanBeInteracted(Interactable interactable) {
        if (grabBox.HoldingBox() && interactable.isGrabbable) return false;
        return true;
    }

    public void InteractNearest() {
        if (GrabNearestIfPossible()) return;
        closestInteractable.Interact();
    }

    public void CancelNearest() {
        if (GrabNearestIfPossible()) return;
        closestInteractable.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if the nearest interactable was a box and was grabbed</returns>
    bool GrabNearestIfPossible() {
        if (closestInteractable.isGrabbable) {
            Grabbable grabbable = closestInteractable.GetComponent<Grabbable>();
            grabBox.Grab(grabbable);
            return true;
        }

        return false;
    }
}