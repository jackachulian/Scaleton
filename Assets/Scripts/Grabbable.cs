using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : Interactable
{
    private float timeReleased;

    public new readonly bool isGrabbable = true;

    // If thrown/dropped less than 0.5s ago, cannot be jumped off of (prevents flying glitch)
    public bool CanBeJumpedOff() {
        return timeReleased+0.25f < Time.time;
    }

    public void Release() {
        timeReleased = Time.time;
    }

    public override void Interact()
    {
        // this will not be called, custom handling for grabbables in Interaction script on player.)
    }
}
