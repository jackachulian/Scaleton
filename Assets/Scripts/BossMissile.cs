using System;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class BossMissile : DamageableEntity {
    [SerializeField] private GameObject hitEffect;

    // in degrees
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private float speed = 8f;

    [SerializeField] private float rotateDelay = 0.75f;

    private PlayerController target;
    private PresidentBoss sender;
    private Rigidbody2D rb;

    bool reflected;

    protected override void Awake() {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if (target == null) return;

        if (rotateDelay < 0) {
            float targetHeight = reflected ? 3.5f : 1.5f;
            Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position) + (Vector2.up * targetHeight * 0.5f);
            Vector2 targetDirection = targetPosition - (Vector2)transform.position;
            rb.velocity = Vector3.RotateTowards(rb.velocity, targetDirection.normalized * speed, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
        } else {
            rotateDelay -= Time.fixedDeltaTime;
        }
        transform.right = rb.velocity.normalized;
    }

    // only self destruct if object collided with was not a boss crate
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<BossCrate>()) return;
        if (hitEffect) {
            var effect = Instantiate(hitEffect, transform.position, transform.rotation);
            var hurtbox = effect.GetComponent<DamageHurtbox>();
            hurtbox.SetDamagePlayer(!reflected);
            hurtbox.DealDamage();
        }
        Destroy(gameObject);
    }

    public void Initialize(PlayerController target, PresidentBoss sender, Vector2 initialDirection) {
        this.target = target;
        this.sender = sender;

        // Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position);
        // Vector2 targetDirection = targetPosition - (Vector2)transform.position;
        rb.velocity = initialDirection.normalized * speed;
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