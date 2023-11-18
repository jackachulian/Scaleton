using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : Interactable
{
    private float timeReleased;

    private AudioSource audioSource;

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

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.relativeVelocity.magnitude > 5f) {
            if (!audioSource) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = 30f;
            }

            SoundManager.PlaySound(audioSource, "boxcollision");
        }
    }
}
