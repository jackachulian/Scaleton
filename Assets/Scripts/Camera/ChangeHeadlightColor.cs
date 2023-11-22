using UnityEngine;

// Intended to be put on CameraRooms. Changes the color of the player's headlight.
// Used to make the player's headlight less yellow when entering the tech area
// because the yellow looks bad on the steel

public class ChangeHeadlightColor : MonoBehaviour {
    [SerializeField] private Color headlightColor;

    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (!playerController) return;

        playerController.headlight.SetColor(headlightColor);
    }
}