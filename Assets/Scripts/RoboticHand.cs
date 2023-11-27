using Unity.VisualScripting;
using UnityEngine;

// Will latch onto grabbables. Player can grab the box out of this hand.

public class RoboticHand : Respawnable {
    [SerializeField] private Grabbable heldBox;

    private Grabbable initialHeldBox;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite grabSprite, releaseSprite;

    [SerializeField] private Transform holdPosition;

    private Rigidbody2D rb;

    [SerializeField] private bool useJoint;


    private RelativeJoint2D relativeJoint;

    protected override void Awake() {
        base.Awake();
        initialHeldBox = heldBox;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (heldBox) {
            heldBox.GetComponent<Rigidbody2D>().isKinematic = true;
            heldBox.transform.position = holdPosition.position;
        }
        if (useJoint) {
            rb = GetComponent<Rigidbody2D>();
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
        var boxRb = g.GetComponent<Rigidbody2D>();
        boxRb.position = holdPosition.position;
        boxRb.angularVelocity = 0f;
        boxRb.rotation = Mathf.Round(boxRb.rotation/90f)*90f; // snap rotation to closest 90deg increment
        spriteRenderer.sprite = grabSprite;

        if (useJoint) {
            boxRb.velocity = rb.velocity;
            relativeJoint = rb.GetComponent<RelativeJoint2D>();
            if (!relativeJoint) relativeJoint = rb.AddComponent<RelativeJoint2D>();
            relativeJoint.connectedBody = boxRb;
        } else {
            boxRb.velocity = Vector2.zero;
            boxRb.isKinematic = true;
        }
    }

    public void ReleaseBox() {
        spriteRenderer.sprite = releaseSprite;
        if (useJoint && relativeJoint) Destroy(relativeJoint);
        if (!heldBox) return;
        heldBox.GetComponent<Rigidbody2D>().isKinematic = false;
        heldBox = null;
    }

    public override void Respawn()
    {
        base.Respawn();
        if (heldBox && heldBox != initialHeldBox) ReleaseBox();
    }
}