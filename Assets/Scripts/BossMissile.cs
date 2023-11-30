using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class BossMissile : Projectile {
    // in degrees
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private float speed = 8f;

    [SerializeField] private float reflectSpeed = 11f;

    [SerializeField] private float rotateDelay = 0.75f;

    private PlayerController target;
    private PresidentBoss sender;
    private Rigidbody2D rb;
    bool reflected;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (!gameObject) return;

        if (target && target.IsDead()) target = null;

        if (target) {
            if (rotateDelay < 0) {
                float targetHeight = reflected ? 2f : 0.75f;
                Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position) + (Vector2.up * targetHeight * 0.5f);
                Vector2 targetDirection = targetPosition - (Vector2)transform.position;
                rb.velocity = Vector3.RotateTowards(rb.velocity, targetDirection.normalized * speed, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
            } else {
                rotateDelay -= Time.fixedDeltaTime;
            }
        }
        
        transform.right = rb.velocity.normalized;
    }

    // only self destruct if object collided with was not a boosting boss crate
    // if a boosting boss crate hits this. reflect this missile back towards the boss
    protected override void OnCollisionEnter2D(Collision2D other) {
        var bossCrate = other.gameObject.GetComponent<BossCrate>();
        if (bossCrate && bossCrate.boosting) {
            Reflect();
        } else {
            ProjHit();
        }
    }
    public void Initialize(PlayerController target, PresidentBoss sender, Vector2 initialDirection) {
        this.target = target;
        this.sender = sender;

        // Vector2 targetPosition = (Vector2)(reflected ? sender.transform.position : target.transform.position);
        // Vector2 targetDirection = targetPosition - (Vector2)transform.position;
        rb.velocity = initialDirection.normalized * speed;
    }

    private void Reflect() {
        if (reflected) return;

        rb.velocity = -rb.velocity;

        reflected = true;
        speed = reflectSpeed;
        destroyTimer = maxTime*2f;

        foreach (var crate in sender.GetBossCrates()) {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), crate.GetComponent<Collider2D>(), true);
        }
    }

    protected override void ProjHit()
    {
        if (hitEffect) {
            var effect = Instantiate(hitEffect, transform.position, transform.rotation);
            var hurtbox = effect.GetComponent<DamageHurtbox>();
            hurtbox.SetDamagePlayer(!reflected);
            hurtbox.DealDamage();
            Destroy(gameObject);
        }        
    }
}