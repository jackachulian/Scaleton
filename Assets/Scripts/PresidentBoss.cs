using System;
using Unity.Mathematics;
using UnityEngine;

public class PresidentBoss : MonoBehaviour {
    // inputs
    [SerializeField] private Transform jumpHeightTransform;

    [SerializeField] private Transform[] jumpTargets;

    [SerializeField] private AnimationCurve jumpHeightCurve;
    
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
        rb = GetComponent<Rigidbody2D>();
        facing = -1;
    }

    private void Start() {
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

    public void IdleUpdate() {
        idleTimeRemaining -= Time.fixedDeltaTime;
        if (idleTimeRemaining < 0) {
            JumpPrepare();
        }
    }

    // per update handlers for each phase
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

    // functions to initate a new phase
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
        if (Mathf.Sign(jumpingTo.x - rb.position.x) != facing) {
            Flip();
        }
    }

    public void Jump() {
        Debug.Log("Jump phase entered ===================================");
        phase = BossPhase.Jump;

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

    public void Idle() {
        Debug.Log("Idle phase entered ===================================");
        phase = BossPhase.Idle;
        idleTimeRemaining = UnityEngine.Random.Range(frameData.idleTimeMin, frameData.idleTimeMax);
    }

    // collision - ussed for jump -> jumpLand
    private void OnCollisionEnter2D(Collision2D other) {
        if (phase == BossPhase.JumpFall) {
            JumpLand();
        }
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