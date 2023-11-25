using UnityEngine;

// script for a lantern that can be toggled on and off
public class Lantern : Respawnable {
    bool on;
    int power; // amount of things powering this door to open. will open if 1 or higher and close if 0 or somehow lower

    Animator animator;


    AudioSource audioSource;
    
    protected override void Awake()  {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Power() {
        power++;

        if (power > 0 && !on) {
            on = true;
            animator.SetBool("on", true);
            // if (Time.time > 1f) SoundManager.PlaySound(audioSource, "light_on");
        }
    }

    public void Unpower() {
        power--;

        if (power == 0 && on) {
            on = false;
            animator.SetBool("on", false);
            // if (Time.time > 1f) SoundManager.PlaySound(audioSource, "light_off");
        }
    }

    public override void Respawn()
    {
        Unpower();
        power++;
    }
}