using UnityEngine;

public class SteppingSounds : MonoBehaviour {
    [SerializeField] private PlayerController playerController;

    // (This method is called by Scaleton's walking animations via an animation event, playing when the foot touches the ground)
    public void SteppingSound() {
        if (playerController.IsGrounded) SoundManager.PlaySound(playerController.audioSource, "step");
    }
}