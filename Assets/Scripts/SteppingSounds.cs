using UnityEngine;

public class SteppingSounds : MonoBehaviour {
    [SerializeField] private PlayerController playerController;


    public void SteppingSound() {
        if (playerController.IsGrounded) SoundManager.PlaySound(playerController.audioSource, "step");
    }
}