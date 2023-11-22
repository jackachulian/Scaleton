using UnityEngine;

public class PlatformTrack : MonoBehaviour {
    [SerializeField] private Transform start, end, platforms;

    [SerializeField] private float speed = 1.5f;

    [SerializeField] private Rigidbody2D[] platformRbs;

    private Vector2 startToEndOffset;

    private void Awake() {
        startToEndOffset = end.position - start.position;
        platformRbs = platforms.GetComponentsInChildren<Rigidbody2D>();
    }

    private void Start() {
        foreach (var rb in platformRbs) {
            rb.velocity = (Vector2)(end.position - start.position).normalized * speed;
        }
    }

    private void FixedUpdate() {
        foreach (var rb in platformRbs) {
            var rbToEndOffset = (Vector2)end.position - rb.position;
            if (Vector2.Dot(startToEndOffset, rbToEndOffset) < 0f) {
                rb.position = (Vector2)start.position + rbToEndOffset;
            }
        }
    }
}