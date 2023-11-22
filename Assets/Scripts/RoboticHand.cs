using UnityEngine;

// Will latch onto grabbables. Player can grab the box out of this hand.

public class RoboticHand : MonoBehaviour {
    [SerializeField] private Grabbable heldBox;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite grabSprite, releaseSprite;

    [SerializeField] private Transform holdPosition;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (heldBox) {
            heldBox.GetComponent<Rigidbody2D>().isKinematic = true;
            heldBox.transform.position = holdPosition.position;
        }
    }

    private void Start() {
        if (heldBox) Grab(heldBox);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (heldBox) return;
        Grabbable g = other.GetComponent<Grabbable>();
        if (!g) return;

        Grab(g);
    }

    private void Grab(Grabbable g) {
        heldBox = g;
        g.AttachToRoboticHand(this);
        var rb = g.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.position = holdPosition.position;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = Mathf.Round(rb.rotation/90f)*90f; // snap rotation to closest 90deg increment
        spriteRenderer.sprite = grabSprite;
    }

    public void ReleaseBox() {
        spriteRenderer.sprite = releaseSprite;
        heldBox.GetComponent<Rigidbody2D>().isKinematic = false;
        heldBox = null;
    }
}