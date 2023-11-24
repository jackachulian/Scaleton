using UnityEngine;

public abstract class Interactable : MonoBehaviour {
    // Little notification to show to press X. If null, no notification is shown
    [SerializeField] private GameObject inputHint;

    // Sprite renderer - its material will be set to outline when nearby (optional)
    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer{get{return spriteRenderer;}}

    [SerializeField] private bool isGrabbable = false;
    public bool IsGrabbable {get{return isGrabbable;}}
    
    // When X is pressed
    public abstract void Interact();

    // When C is pressed (optional)
    public virtual void Cancel() {}

    public virtual void Hover() {
        if (inputHint) inputHint.SetActive(true);
    }

    public virtual void Unhover() {
        if (inputHint) inputHint.SetActive(false);
    }

    public void SetGrabbable (bool grabbable) {
        isGrabbable = grabbable;
    }
}