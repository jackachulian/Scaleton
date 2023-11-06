using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour {
    
    // Sprite renderer - its material will be set to outline when nearby (optional)
    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer{get{return spriteRenderer;}}

    [SerializeField] private bool isGrabbable = false;
    public bool IsGrabbable {get{return isGrabbable;}}

    // When X is pressed
    public abstract void Interact();

    // When C is pressed (optional)
    public virtual void Cancel() {}

    // When walking towards interactable (optional)
    public virtual void Hover() {}

    // When walking away from interactable (optional)
    public virtual void Unhover() {}
}