using UnityEngine;

public abstract class DamageableEntity : Respawnable {
    [SerializeField] protected int _hp = 50;
    public int hp {get{return _hp;}}

    [SerializeField] private bool isPlayer;

    public int maxHp {get; private set;}

    public abstract void OnHit(int dmg, DamageHurtbox hurtbox);

    public abstract void Die();

    protected override void Awake() {
        base.Awake();
        maxHp = _hp;
    }

    public bool IsPlayer() {
        return isPlayer;
    }

    public override void Respawn()
    {
        base.Respawn();
        _hp = maxHp;
    }
}