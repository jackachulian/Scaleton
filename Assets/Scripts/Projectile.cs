using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField] protected GameObject hitEffect;

    [SerializeField] protected float maxTime = 5f;

    protected float destroyTimer;

    private void Start() {
        destroyTimer = maxTime;
    }

    protected virtual void FixedUpdate() {
        destroyTimer -= Time.fixedDeltaTime;
        if (destroyTimer < 0) {
            ProjHit();
            return;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {
        Debug.LogWarning("(base proj class) collided with "+other.gameObject.name);
        ProjHit();
    }

    protected virtual void ProjHit() {
        Instantiate(hitEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}