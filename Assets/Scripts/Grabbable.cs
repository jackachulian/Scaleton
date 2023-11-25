using UnityEngine;

public class Grabbable : Interactable
{
    private float timeReleased;

    public AudioSource audioSource {get; private set;}

    // if not held by player, the hand that is grabbing this box
    private RoboticHand roboticHand;

    protected void Awake() {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // If thrown/dropped less than 0.5s ago, cannot be jumped off of (prevents flying glitch)
    // Will only be used if preventPropFly bool is set to true in PlayerController
    public bool CanBeJumpedOff() {
        return timeReleased+0.25f < Time.time;
    }

    public virtual void Release() {
        timeReleased = Time.time;
    }

    public void AttachToRoboticHand(RoboticHand hand) {
        roboticHand = hand;
    }

    public void DetachFromRoboticHand() {
        if (!roboticHand) return;
        roboticHand.ReleaseBox();
        roboticHand = null;
    }

    public override void Interact()
    {
        // this will not be called, custom handling for grabbables in Interaction script on player.)
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {
        // don't play sounds within the first ~1s - this is when blocks are first falling nto the level
        if (Time.time < 1.5f) return;

        if (other.relativeVelocity.magnitude > 5f) {
            SoundManager.PlaySound(audioSource, "boxcollision");
        }
    }
}
