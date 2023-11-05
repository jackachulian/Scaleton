using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    private float timeReleased;


    // If thrown/dropped less than 0.5s ago, cannot be jumped off of (prevents flying glitch)
    public bool CanBeJumpedOff() {
        return timeReleased+0.25f < Time.time;
    }

    public void Release() {
        timeReleased = Time.time;
    }
}
