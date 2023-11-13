using UnityEngine;

public class RespawnPoint : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (!playerController) return;

        CameraRoom currentRoom = playerController.GetCurrentRoom();
        if (currentRoom) currentRoom.SetRespawnPoint(transform);
    }
}