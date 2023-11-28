using UnityEngine;

public class DamageHurtbox : MonoBehaviour {
    [SerializeField] private bool damageOnAwake;

    [SerializeField] private HurtboxShape hurtboxShape;
    public enum HurtboxShape {
        Circle,
        Box,
    }

    [SerializeField] private float radius = 1.5f;

    [SerializeField] private Vector2 boxSize = Vector2.one;

    [SerializeField] private int damage = 1;

    // Impulse force applied from the center of this transform on damage
    [SerializeField] private float forceMagnitude = 2f;

    // If true, this will deal damage to the player.
    // If false, it will deal damage to enemies.
    [SerializeField] private bool damagePlayer;

    private void Awake() {
        if (damageOnAwake) DealDamage();
    }

    public void DealDamage() {
        RaycastHit2D[] hits;

        if (hurtboxShape == HurtboxShape.Circle) {
            hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        
        } else {
            hits = Physics2D.BoxCastAll(transform.position, boxSize, 0f, Vector2.zero);
        }

        foreach (var hit in hits) {
            if (hit.collider.isTrigger) continue;
            var entity = hit.transform.GetComponent<DamageableEntity>();
            if (entity) {
                if (damagePlayer == entity.IsPlayer()) entity.OnHit(damage, this);
            } 

            // apply force regardless if should damage or not
            if (forceMagnitude <= 0) continue;
            var otherRb = hit.transform.GetComponent<Rigidbody2D>();
            if (!otherRb) continue;
            Vector2 force = (otherRb.position - (Vector2)transform.position).normalized * forceMagnitude;
            otherRb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        if (hurtboxShape == HurtboxShape.Box) {
            Gizmos.DrawWireCube(transform.position, boxSize);
        } else {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    public float GetRadius() {
        if (hurtboxShape == HurtboxShape.Circle) {
            return radius;
        } else {
            return Mathf.Sqrt(boxSize.x*boxSize.x + boxSize.y*boxSize.y);
        }
    }
}