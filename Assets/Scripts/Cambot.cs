using UnityEngine;

public class Cambot : DamageableEntity
{
    [SerializeField] private Animator animator;

    [SerializeField] private Rigidbody2D rb;

    public override void Die()
    {
        animator.SetBool("dead", true);
        rb.isKinematic = false;
    }

    public override void OnHit(int dmg, DamageHurtbox hurtbox)
    {
        _hp -= dmg;
        if (hp <= 0) {
            Die();
        }
        else {
            // idk what to do here cambot should have only 1 hp
        }
    }
}