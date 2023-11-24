using UnityEngine;

public abstract class DamageableEntity : MonoBehaviour {
    [SerializeField] protected int hp = 50;

    public abstract void OnHit(int dmg, DamageHurtbox hurtbox);

    public abstract void Die();
}