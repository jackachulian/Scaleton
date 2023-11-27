using UnityEngine;

public class FollowPath : Respawnable {
    [SerializeField] private Transform[] points;

    [SerializeField] private float speed = 1.5f;
    [SerializeField] private Rigidbody2D rb;
    int pointIndex;
    private Vector2 offsetToNextPoint;

    private Transform nextPoint;

    [SerializeField] private Transform spriteTransform;

    [SerializeField] bool faceMove;

    int facing = 1;

    protected override void Awake() {
        if (points.Length == 0) {
            enabled = false;
            return;
        }

        rb.position = points[0].position;

        if (points.Length == 1) {
            enabled = false;
            return;
        }

        nextPoint = points[1];
        rb.position = points[0].position;
        MoveTowardsCurrentPoint();
    }

    private void FixedUpdate() {
        var offset = (Vector2)nextPoint.position - rb.position;
        if (Vector2.Dot(offsetToNextPoint, offset) < 0f) {
            pointIndex++;
            MoveTowardsCurrentPoint();
        }
    }

    private void MoveTowardsCurrentPoint() {
        nextPoint = points[(pointIndex+1) % points.Length];
        offsetToNextPoint = (Vector2)nextPoint.position - rb.position;
        rb.velocity = offsetToNextPoint.normalized * speed;

        if (faceMove) {
            if (facing != Mathf.Sign(offsetToNextPoint.x)) Flip();
        }
    }

    public override void Respawn()
    {
        base.Respawn();
        if (!enabled) return;
        pointIndex = 0;
        rb.position = points[0].position;
        MoveTowardsCurrentPoint();
    }

    public void Flip() {
        facing = -facing;
        spriteTransform.Rotate(0.0f, 180.0f, 0.0f);
    }
}