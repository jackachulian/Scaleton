using System;
using Unity.Mathematics;
using UnityEngine;

public class PresidentBoss : MonoBehaviour {
    // inputs
    [SerializeField] private Transform jumpHeightTransform;

    [SerializeField] private Transform[] jumpTargets;

    [SerializeField] private AnimationCurve jumpHeightCurve;

    /// <summary> Colliders that are ignored during the jump phase, but not the jumpfall phase </summary>
    [SerializeField] private Collider2D[] ignoreCollidersDuringJump;
    
    // frame data
    [SerializeField] private FrameData frameData;
    /// <summary>For frame data not contained within the animations themselves.</summary>
    [Serializable] public class FrameData {
        public float idleTimeMin = 3f;
        public float idleTimeMax = 7f;
        /// <summary> Aerial hang time before fall begins </summary>
        public float jumpAerialTime = 0.5f;
        /// <summary> Extra aerial hang time per game unit of X travel distance </summary>
        public float jumpHorizontalExtraAerialTime = 0.05f;
        /// <summary> Amount of time between fall start and hitting the ground </summary>
        public float jumpFallTime = 0.6f;
        /// <summary> Target Y velocity of instant force applied when falling </summary>
        public float jumpFallInstantVel = -1f;
    }

    // cached references
    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    private PlayerController player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // internal animation/timing related vars
    /// <summary> Amount of seconds remaining the boss will wait before choosing an attack phase </summary>
    private float idleTimeRemaining;

    // other internal variables
    /// <summary> What action the boss is currently taking. </summary>
    private BossPhase phase;
    /// <summary> 1 for facing right, -1 for facing left </summary>
    private int facing;
    /// <summary> During Jump phase, the two points the boss is jumping between. </summary>
    private Vector2 jumpingFrom, jumpingTo;


    private void Awake() {
        animator = GetComponent<Animator>();
        cc = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        facing = -1;
    }

    private void Start() {
        player = MenuManager.player;

        Idle();
    }

    private void FixedUpdate() {
        switch (phase) {
            case BossPhase.Idle: IdleUpdate(); break;
            case BossPhase.JumpPrepare: UpdateJumpPrepare(); break;
            case BossPhase.Jump: UpdateJump(); break;
            case BossPhase.JumpFall: UpdateJumpFall(); break;
            case BossPhase.JumpLand: UpdateJumpLand(); break;
            case BossPhase.BladeSlashPrepare: break;
            case BossPhase.BladeSlash: break;
            case BossPhase.MissileLaunchPrepare: break;
            case BossPhase.MissileLaunch: break;
        }
    }

    // per update handlers for each phase
    public void IdleUpdate() {
        FaceTowards(player.transform.position);

        idleTimeRemaining -= Time.fixedDeltaTime;
        if (idleTimeRemaining < 0) {
            JumpPrepare();
        }

    }

    public void UpdateJumpPrepare() {
        // once animator has left jumpPrepare anim reached jump anim, initiate jump
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_jump")) {
            Jump();
        }
    }

    public void UpdateJump() {
        if ((rb.position.x - jumpingFrom.x)/(jumpingTo.x - jumpingFrom.x) >= 1f || rb.velocity.y < 0f) {
            JumpFall();
        }
    }

    public void UpdateJumpFall() {
        // no behaviour here - JumpLand will occurr in OnCollisionEnter2D when colliding with something
    }

    public void UpdateJumpLand() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_idle")) {
            Idle();
        }
    }

    // functions to initate a new phase change

    public void Idle() {
        Debug.Log("Idle phase entered ===================================");
        phase = BossPhase.Idle;
        idleTimeRemaining = UnityEngine.Random.Range(frameData.idleTimeMin, frameData.idleTimeMax);
    }

    public void JumpPrepare() {
        Debug.Log("Jump prepare phase entered ===================================");
        phase = BossPhase.JumpPrepare;
        animator.CrossFade("presidentboss_jumpprepare", 0f);
        
        jumpingTo = jumpTargets[UnityEngine.Random.Range(0, jumpTargets.Length)].position;
        int iter = 0;
        Vector2 jumpingFrom = jumpingTo;
        while (jumpingTo == jumpingFrom && iter < 100) {
            jumpingTo = jumpTargets[UnityEngine.Random.Range(0, jumpTargets.Length)].position;
            iter++;
        }
        FaceTowards(jumpingTo);
    }

    public void Jump() {
        Debug.Log("Jump phase entered ===================================");
        phase = BossPhase.Jump;

        foreach (var c in ignoreCollidersDuringJump) {
            Physics2D.IgnoreCollision(cc, c, true);
        }

        jumpingFrom = rb.position;
        float xOffset = jumpingTo.x - jumpingFrom.x;
        float aerialTime = frameData.jumpAerialTime + Math.Abs(xOffset * frameData.jumpHorizontalExtraAerialTime);
        float jumpHeight = jumpHeightTransform.position.y - rb.position.y;
        // physics time:
        // final velocity of 0 needed
        // know: t, v, s
        // need: u, a
        // s = 1/2(v + u)t
        // jumpHeight = 0.5f * (0 + initialVelocity) * aerialTime
        var initialYVelocity = jumpHeight * 2f / aerialTime;

        // v = u + at
        // 0 = initialVelocity + gravity * frameData.jumpFallTime
        float gravity = -initialYVelocity / aerialTime;
        rb.gravityScale = gravity/Physics2D.gravity.y;

        rb.AddForce(initialYVelocity * Vector2.up * rb.mass, ForceMode2D.Impulse);

        // Apply a horizontal force that will make the boss arrive at the target position after aerialTime seconds
        float targetXVelocity = xOffset / aerialTime;
        float xVelocityDelta = targetXVelocity - rb.velocity.x;
        rb.AddForce(xVelocityDelta * Vector2.right * rb.mass, ForceMode2D.Impulse);
    }

    public void JumpFall() {
        Debug.Log("Jump fall phase entered ===================================");
        phase = BossPhase.JumpFall;
        animator.CrossFade("presidentboss_jumpfall", 0f);

        foreach (var c in ignoreCollidersDuringJump) {
            Physics2D.IgnoreCollision(cc, c, false);
        }

        float fallDistance = jumpingTo.y - rb.position.y;

        Vector2 targetVelocity = new Vector2(0, frameData.jumpFallInstantVel);
        rb.AddForce((targetVelocity - rb.velocity) * rb.mass, ForceMode2D.Impulse);

        // physics time!
        // know: s, u, t
        // need: v, a
        // s = 1/2(v + u)t
        // fallDistance = 0.5f * (finalVelocity + rb.velocity.y) * frameData.jumpFallTime
        var finalVelocity = fallDistance / (frameData.jumpFallTime * 0.5f) - rb.velocity.y;

        // v = u + at
        // finalVelocity = rb.velocity.y + gravity * frameData.jumpFallTime
        float gravity = (finalVelocity - rb.velocity.y) / frameData.jumpFallTime;
        rb.gravityScale = gravity/Physics2D.gravity.y;
    }

    public void JumpLand() {
        Debug.Log("Jump land phase entered ===================================");
        phase = BossPhase.JumpLand;
        animator.CrossFade("presidentboss_jumpland", 0f);
    }

    // collision - ussed for jump -> jumpLand
    private void OnCollisionEnter2D(Collision2D other) {
        if (phase == BossPhase.JumpFall) {
            JumpLand();
        }

        CheckDamage(other);
    }

    public void CheckDamage(Collision2D other) {
        // Take damage only from grabbables (for now) (TODO other damage types, maybe not in this function)
        Grabbable grabbable = other.gameObject.GetComponent<Grabbable>();
        if (!grabbable) return;

        // Relative velocity magnitude must be high enough
        if (other.relativeVelocity.magnitude < 1.5f) return;

        // Don't take damage if normal points down at all and box's velocity is not positive, meaning box was stepped on
        if (other.GetContact(0).normal.y < 0f && grabbable.GetComponent<Rigidbody2D>().velocity.y < 0.25f) return;

        // If all steps pass, this box will deal damage
        // TODO: implement damage
        Debug.Log("Damage dealt");
    }

    // misc
    public void Flip() {
        facing *= -1;
        spriteRenderer.transform.Rotate(0.0f, 180.0f, 0.0f);
        spriteRenderer.transform.localPosition =  new Vector3(
            -spriteRenderer.transform.localPosition.x, 
            spriteRenderer.transform.localPosition.y,
            spriteRenderer.transform.localPosition.z);
    }

    public void FaceTowards(Vector2 point) {
        if (Mathf.Sign(point.x - rb.position.x) != facing) {
            Flip();
        }
    }
}

public enum BossPhase {
    Idle,
    JumpPrepare,
    Jump,
    JumpFall,
    JumpLand,
    BladeSlashPrepare,
    BladeSlash,
    MissileLaunchPrepare,
    MissileLaunch
}