using UnityEngine;

public abstract class DamageableEntity : MonoBehaviour {
    [SerializeField] protected int _hp = 50;
    public int hp {get{return _hp;}}

    [SerializeField] private bool isPlayer;

    public int maxHp {get; private set;}

    public abstract void OnHit(int dmg, DamageHurtbox hurtbox);

    public abstract void Die();

    protected virtual void Awake() {
        maxHp = _hp;
    }

    public bool IsPlayer() {
        return isPlayer;
    }
}