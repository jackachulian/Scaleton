using System.Collections;
using UnityEngine;

public class Cambot : DamageableEntity
{
    [SerializeField] private Animator animator;

    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private GameObject scanLight;

    [SerializeField] private FollowPath followPath;

    [SerializeField] private float trackedFollowSpeed = 0.8f;

    [SerializeField] private GameObject bombProjectile;

    [SerializeField] private float bombSpeed;

    [SerializeField] private Transform bombShootTransform;
    [SerializeField] private float bombShootDelay = 0.8f;
    [SerializeField] private float undetectDelay = 0.8f;

    [SerializeField] private LayerMask obstructionLayerMask;


    private PlayerController trackedPlayer;
    private Vector2 targetPosition;
    private Collider2D cc;
    private float bombShootTimer;
    private float undetectTimer;

    bool dead = false;

    protected override void Awake() {
        base.Awake();
        cc = GetComponent<Collider2D>();
    }

    private void FixedUpdate() {
        // only control player tracking movement in this fixed update, followpath while not tracking
        if (!trackedPlayer) return;

        if (trackedPlayer.IsDead()) {
            Undetect();
            return;
        }

        if (CanSee(trackedPlayer.transform)) {
            targetPosition = trackedPlayer.transform.position;
            undetectTimer = undetectDelay;
        } else {
            if (undetectTimer < 0f) {
                Undetect();
                return;
            }
            undetectTimer -= Time.fixedDeltaTime;
        }

        Vector2 targetVelocity = (targetPosition - (Vector2)transform.position).normalized * trackedFollowSpeed;
        rb.velocity = targetVelocity;

        if (bombShootTimer < 0f) {
            bombShootTimer += bombShootDelay;
            ShootBomb();
        }
        bombShootTimer -= Time.fixedDeltaTime;
    }

    private void ShootBomb() {
        var playerOffset = (Vector2)trackedPlayer.transform.position - (Vector2)transform.position;
        GameObject bomb = Instantiate(bombProjectile, transform.position, Quaternion.identity);
        bomb.transform.right = playerOffset.normalized;

        Collider2D bombCollider = bomb.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(cc, bombCollider);

        Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
        bombRb.velocity = playerOffset.normalized * bombSpeed;
    }

    public override void Die()
    {
        dead = true;
        rb.gravityScale = 1f;

        gameObject.layer = LayerMask.NameToLayer("IntangibleObject");
        scanLight.SetActive(false);
        trackedPlayer = null;
        followPath.enabled = false;
        StartCoroutine(DestroyCorpseAfterDelay());
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

    // will play the falling on ground anim when hitting the ground - will only collidw with ground after dead and layer set to IntangibleObject
    private void OnCollisionEnter2D(Collision2D other) {
        if (dead && other.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            animator.SetBool("dead", true);
        }
    }

    IEnumerator DestroyCorpseAfterDelay() {
        yield return new WaitForSeconds(7.5f);
        Destroy(gameObject);
    }

    public void Detect(PlayerController player) {
        if (dead) return;
        if (!CanSee(player.transform)) return;
        trackedPlayer = player;
        bombShootTimer = bombShootDelay;
        followPath.enabled = false;
    }

    public void Undetect() {
        trackedPlayer = null;
        followPath.enabled = true;
        followPath.MoveTowardsCurrentPoint();
    }

    public bool CanSee(Transform other, float height = 1.5f) {
        var topHit = Physics2D.Linecast((Vector2)bombShootTransform.position, (Vector2)other.position + (Vector2.up * height * 0.5f), obstructionLayerMask);
        var bottomHit = Physics2D.Linecast((Vector2)bombShootTransform.position, (Vector2)other.position - (Vector2.up * height * 0.5f), obstructionLayerMask);
        return !topHit || !bottomHit;
    }

    public override void Respawn()
    {
        if (dead) return;
        base.Respawn();
    }
}