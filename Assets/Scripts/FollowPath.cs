using UnityEngine;

public class FollowPath : MonoBehaviour {
    [SerializeField] private Transform[] points;

    [SerializeField] private float speed = 1.5f;
    [SerializeField] private Rigidbody2D rb;
    int pointIndex;
    private Vector2 offsetToNextPoint;

    private Transform point, nextPoint;

    private void Awake() {
        if (points.Length == 0) {
            enabled = false;
            return;
        }

        rb.position = points[0].position;

        if (points.Length == 1) {
            enabled = false;
            return;
        }

        point = points[0];
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
        point = nextPoint;
        nextPoint = points[pointIndex+1 % points.Length];
        offsetToNextPoint = (Vector2)nextPoint.position - rb.position;
        rb.velocity = offsetToNextPoint.normalized * speed;
    }
}