using UnityEngine;

public class DamageHurtbox : MonoBehaviour {
    [SerializeField] private bool damageOnAwake;

    [SerializeField] private float radius = 1.5f;

    [SerializeField] private int damage = 1;

    private void Awake() {
        if (damageOnAwake) DealDamage();
    }

    public void DealDamage() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        foreach (var hit in hits) {
            var entity = hit.transform.GetComponent<DamageableEntity>();
            if (entity) {
                entity.OnHit(damage, this);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}