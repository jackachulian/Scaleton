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
        public float jumpAerialTime = 1.5f;
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
        facing = 1;
    }

    private void Start() {
        Idle();
    }

    private void FixedUpdate() {
        switch (phase) {
            case BossPhase.Idle: IdleUpdate(); break;
            case BossPhase.JumpPrepare: UpdateJumpPrepare(); break;
            case BossPhase.Jump: UpdateJump(); break;
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
        float xDelta = rb.velocity.x*Time.fixedDeltaTime;
        float nextDistancePercent = facing * (rb.position.x + xDelta - jumpingFrom.x) / (jumpingTo.x - jumpingFrom.x);

        if (facing * nextDistancePercent < 1f) {
            float jumpHeightPercent = jumpHeightCurve.Evaluate(nextDistancePercent);
            float nextUpdateY = Mathf.LerpUnclamped(jumpingFrom.y, jumpHeightTransform.position.y, jumpHeightPercent);
            float yDelta = nextUpdateY - rb.position.y;
            rb.AddForce(yDelta * rb.mass * Vector2.up, ForceMode2D.Impulse);
        }
        // don't apply any addition forces if past target x, gravity and existing momentum will bring boss to ground,
        // jumpLand phase will start when OnCollisionEnter2D is triggered by colliding with ground/player
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
        while (jumpingTo.x == rb.position.x) {
            jumpingTo = jumpTargets[UnityEngine.Random.Range(0, jumpTargets.Length)].position;
        }
        if (Mathf.Sign(jumpingTo.x - transform.position.x) != facing) {
            Flip();
        }
    }

    public void Jump() {
        Debug.Log("Jump phase entered ===================================");
        phase = BossPhase.Jump;
        jumpingFrom = transform.position;
        float xOffset = jumpingTo.x - jumpingFrom.x;
        float targetXVelocity = xOffset / frameData.jumpAerialTime;
        float xVelocityDelta = targetXVelocity - rb.velocity.x;
        rb.AddForce(xVelocityDelta * Vector2.right * rb.mass, ForceMode2D.Impulse);
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
        if (phase == BossPhase.Jump) {
            JumpLand();
        }
    }

    // misc
    public void Flip() {
        facing *= -1;
        spriteRenderer.transform.Rotate(0.0f, 180.0f, 0.0f);
        spriteRenderer.transform.localPosition.Set(
            -spriteRenderer.transform.localPosition.x, 
            spriteRenderer.transform.localPosition.y,
            spriteRenderer.transform.localPosition.z);
    }
}

public enum BossPhase {
    Idle,
    JumpPrepare,
    Jump,
    JumpLand,
    BladeSlashPrepare,
    BladeSlash,
    MissileLaunchPrepare,
    MissileLaunch
}