using UnityEngine;

public class CambotScan : MonoBehaviour {
    [SerializeField] private Cambot cambot;

    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController player = other.GetComponent<PlayerController>();
        if (!player) return;

        cambot.Detect(player);
    }
}