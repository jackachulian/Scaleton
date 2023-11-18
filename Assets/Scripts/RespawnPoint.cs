using UnityEngine;

public class RespawnPoint : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController player = other.GetComponent<PlayerController>();
        if (!player) return;
        if (player.IsDead()) return;

        CameraRoom currentRoom = player.GetCurrentRoom();
        if (currentRoom) currentRoom.SetRespawnPoint(this);
    }
}