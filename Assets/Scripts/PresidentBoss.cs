using System;
using System.Collections;
using System.Linq;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class PresidentBoss : DamageableEntity {
    // inputs
    [SerializeField] private Transform jumpHeightTransform;

    [SerializeField] private Transform[] jumpTargets;

    [SerializeField] private Transform flipTransform;

    /// <summary> Colliders that are ignored during the jump phase, but not the jumpfall phase </summary>
    [SerializeField] private Collider2D[] ignoreCollidersDuringJump;

    [SerializeField] private Transform headPositionTransform, feetPositionTransform, missileShootTransform;

    [SerializeField] private GameObject landHurtboxAndEffect;

    [SerializeField] private CinemachineImpulseSource landImpulseSource;

    [SerializeField] private GameObject bladeSlashEffect;

    [SerializeField] private GameObject missileObject;

    [SerializeField] private Cutscene deathCutscene, timeOverCutscene;

    // Time (seconds) this boss must me beaten in before timeOverCutscene is played
    [SerializeField] private int time = 180;
    
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

        public float bladeSlashEffectDelay =0.125f;

        public float bladeSlashForwardVelocity = 6f;

        public float missileLaunchSpeed = 5f;
    }

    // cached references
    private AudioSource audioSource;
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    private PlayerController player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // internal animation/timing related vars
    /// <summary> Amount of seconds remaining the boss will wait before choosing an attack phase </summary>
    private float idleTimeRemaining;

    /// <summary>
    /// True if boss is currently idling.
    /// Different than being on idle phase.
    /// If hit during idle, cooldown will not restart.
    /// If hit during an interuptable attack, switch to idle after hit and cooldown restarts
    /// </summary>
    private bool onIdleCooldown;

    // other internal variables
    /// <summary> What action the boss is currently taking. </summary>
    private BossPhase phase;
    /// <summary> 1 for facing right, -1 for facing left </summary>
    private int facing;
    /// <summary> During Jump phase, the two points the boss is jumping between. </summary>
    private Vector2 jumpingFrom, jumpingTo;


    protected override void Awake() {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        cc = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        facing = 1;
    }

    private void Start() {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        player.IgnoreCollisionWhileDead(cc);
        player.GetCurrentRoom().SetRespawnType(CameraRoom.RespawnType.FarthestFromBoss);
        MenuManager.bossUI.gameObject.SetActive(true);
        MenuManager.bossUI.SetBoss(this);
        MenuManager.bossUI.StartTimer(time);
        Idle();
    }

    private void FixedUpdate() {
        switch (phase) {
            case BossPhase.Idle: UpdateIdle(); break;
            case BossPhase.Hit: UpdateHit(); break;
            case BossPhase.JumpPrepare: UpdateJumpPrepare(); break;
            case BossPhase.Jump: UpdateJump(); break;
            case BossPhase.JumpFall: UpdateJumpFall(); break;
            case BossPhase.JumpLand: UpdateJumpLand(); break;
            case BossPhase.BladeSlashPrepare: UpdateBladeSlashPrepare(); break;
            case BossPhase.BladeSlash: UpdateBladeSlash(); break;
            case BossPhase.MissileLaunchPrepare: UpdateMissilePrepare(); break;
            case BossPhase.MissileLaunch: UpdateMissileLaunch(); break;
        }
    }

    // per update handlers for each phase
    public void UpdateIdle() {
        FaceTowards(player.transform.position);

        // do not tick idle timer while player is in death cooldown
        if (player.IsDead()) return;

        idleTimeRemaining -= Time.fixedDeltaTime;
        if (idleTimeRemaining < 0) {
            onIdleCooldown = false;

            // TODO: cycle between idle, blade, idle, missile, repeat
            var rng = UnityEngine.Random.value;
            if (rng < 0.33f) {
                MissilePrepare();
            } else if (rng < 0.67f) {
                BladeSlashPrepare();
            } else {
                JumpPrepare();
            }
        }
    }

    public void UpdateHit() {
        idleTimeRemaining -= Time.fixedDeltaTime;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_idle")) {
            Idle();
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

    public void UpdateBladeSlashPrepare() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_bladeslash")) {
            BladeSlash();
        }
    }

    public void UpdateBladeSlash() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_idle")) {
            Idle();
        }
    }

    public void UpdateMissilePrepare() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_missileshoot")) {
            MissileLaunch();
        }
    }

    public void UpdateMissileLaunch() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("presidentboss_idle")) {
            Idle();
        }
    }

    // functions to initate a new phase change

    public void Idle() {
        phase = BossPhase.Idle;
        if (!onIdleCooldown) {
            onIdleCooldown = true;
            idleTimeRemaining = UnityEngine.Random.Range(frameData.idleTimeMin, frameData.idleTimeMax);
        }
    }

    public void Hit() {
        // restart idle cooldown with less time when certain phases are interrupted
        if (phase == BossPhase.JumpPrepare) {
            onIdleCooldown = true;
            idleTimeRemaining = 0.6f * UnityEngine.Random.Range(frameData.idleTimeMin, frameData.idleTimeMax);
        }
        else if (phase == BossPhase.Jump) {
            onIdleCooldown = true;
            idleTimeRemaining = 0.8f * UnityEngine.Random.Range(frameData.idleTimeMin, frameData.idleTimeMax);
        }
        
        rb.gravityScale = 1f;

        // modify velocity if hit during certain jump phases
        if (phase == BossPhase.Jump || phase == BossPhase.JumpFall) {
            rb.velocity = new Vector2(0f, Mathf.Min(rb.velocity.y, 0));
        }

        foreach (var c in ignoreCollidersDuringJump) {
            Physics2D.IgnoreCollision(cc, c, false);
        }

        phase = BossPhase.Hit;
        animator.CrossFade("presidentboss_hit", 0f);
    }

    public void JumpPrepare() {
        phase = BossPhase.JumpPrepare;
        animator.CrossFade("presidentboss_jumpprepare", 0f);

        // 75% chance to target closest target to player
        IOrderedEnumerable<Transform> sortedJumpTargets;
        if (UnityEngine.Random.value < 0.75f) {
            Debug.Log("Jumping near player");
            sortedJumpTargets = jumpTargets.OrderBy(target => Vector2.Distance(feetPositionTransform.position, player.transform.position));
        } 
        // 25% chance to choose one at random
        else {
            Debug.Log("Jumping at random");
            sortedJumpTargets = jumpTargets.OrderBy(target => UnityEngine.Random.value);
        }

        // Loop through transforms in order, which are sorted from most preferable to least preferable
        foreach (Transform jumpTarget in sortedJumpTargets) {
            // Ensure boss does not jump too close to where it currently already is
            if (Vector2.Distance((Vector2)jumpTarget.position, (Vector2)feetPositionTransform.position) > 2f) {
                jumpingTo = jumpTarget.position;
                break;
            }
        }
        FaceTowards(jumpingTo);
    }

    public void Jump() {
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
        phase = BossPhase.JumpFall;
        // animator.CrossFade("presidentboss_jumpfall", 0f);

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
        phase = BossPhase.JumpLand;
        animator.CrossFade("presidentboss_jumpland", 0f);

        rb.gravityScale = 1f;

        Instantiate(landHurtboxAndEffect, feetPositionTransform.position, Quaternion.identity);
        landImpulseSource.GenerateImpulseAt(feetPositionTransform.position, Vector2.down*0.25f);
    }

    public void BladeSlashPrepare() {
        phase = BossPhase.BladeSlashPrepare;
        animator.CrossFade("presidentboss_bladeprepare", 0f);
    }

    public void BladeSlash() {
        phase = BossPhase.BladeSlash;

        rb.AddForce(Vector2.right * facing * frameData.bladeSlashForwardVelocity * rb.mass, ForceMode2D.Impulse);

        StartCoroutine(BladeEffectAfterDelay());
    }

    public void MissilePrepare() {
        phase = BossPhase.MissileLaunchPrepare;
        animator.CrossFade("presidentboss_missileprepare", 0f);
    }

    public void MissileLaunch() {
        phase = BossPhase.MissileLaunch;

        Vector2 playerOffset = player.transform.position - missileShootTransform.position;
        var missile = Instantiate(missileObject, missileShootTransform.position, Quaternion.identity);
        missile.transform.right = playerOffset.normalized;
        // missile.GetComponent<Rigidbody2D>().velocity = playerOffset.normalized * frameData.missileLaunchSpeed;
        missile.GetComponent<BossMissile>().Initialize(player, this);
    }

    IEnumerator BladeEffectAfterDelay() {
        yield return new WaitForSeconds(frameData.bladeSlashEffectDelay);

        Instantiate(bladeSlashEffect, transform.position, flipTransform.rotation);
    }

    // collision - ussed for jump -> jumpLand
    private void OnCollisionEnter2D(Collision2D other) {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player) {
            if (phase == BossPhase.Jump || phase == BossPhase.JumpFall) {
                player.Die();
            }
        }

        if (phase == BossPhase.JumpFall) {
            JumpLand();
        }
    }

    // misc
    public void Flip() {
        facing *= -1;
        flipTransform.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void FaceTowards(Vector2 point) {
        if (Mathf.Sign(point.x - rb.position.x) != facing) {
            Flip();
        }
    }

    public override void OnHit(int dmg, DamageHurtbox hurtbox)
    {
        // Take more damage if box is near head
        float distanceFromHead = Vector2.Distance(headPositionTransform.position, hurtbox.transform.position);
        if (distanceFromHead < hurtbox.GetRadius() * 2f + 1f) {
            dmg *= 2;
            SoundManager.PlaySound(audioSource, "bossdamage");
            SoundManager.PlaySound(audioSource, "bossdamageheavy");
        } else {
            SoundManager.PlaySound(audioSource, "bossdamage");
        }

        _hp -= dmg;
        MenuManager.bossUI.HealthBarUpdate();
        if (hp <= 0) {
            Die();
        }
        else {
            Hit();
            
        }
    }

    public override void Die()
    {
        enabled = false;
        player.GetCurrentRoom().SetRespawnType(CameraRoom.RespawnType.First);
        deathCutscene.StartCutscene();
    }

    public void DeathAnimation() {
        //todo: make an actual animation here
        gameObject.SetActive(false);
    }

    public void StartTimeOverCutscene() {
        enabled = false;
        player.GetCurrentRoom().SetRespawnType(CameraRoom.RespawnType.First);
        timeOverCutscene.StartCutscene();
    }

    public override void Respawn()
    {
        base.Respawn();
    }
}

public enum BossPhase {
    Idle,
    Hit,
    JumpPrepare,
    Jump,
    JumpFall,
    JumpLand,
    BladeSlashPrepare,
    BladeSlash,
    MissileLaunchPrepare,
    MissileLaunch
}