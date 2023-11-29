using System;
using UnityEngine;

public class BossMissile : DamageableEntity {
    [SerializeField] private GameObject hitEffect;

    // in degrees
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private float speed = 8f;

    private PlayerController target;
    private PresidentBoss sender;
    private Rigidbody2D rb;

    bool reflected;

    protected override void Awake() {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position);
        Vector2 targetDirection = targetPosition - (Vector2)transform.position;
        rb.velocity = Vector3.RotateTowards(rb.velocity, targetDirection.normalized * speed, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
        transform.right = rb.velocity.normalized;

    }

    // only don't self destruct if object collided with was not a boss crate
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<BossCrate>()) return;
        if (hitEffect) Instantiate(hitEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void Initialize(PlayerController target, PresidentBoss sender) {
        this.target = target;
        this.sender = sender;

        Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position);
        Vector2 targetDirection = targetPosition - (Vector2)transform.position;
        rb.velocity = targetDirection.normalized * speed;
    }

    public override void OnHit(int dmg, DamageHurtbox hurtbox)
    {
        // regardless of anything, just reflect back at the opponent if not already
        if (reflected) return;

        reflected = true;
    }

    public override void Die()
    {
        throw new System.NotImplementedException();
    }
}