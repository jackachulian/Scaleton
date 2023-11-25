using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : DamageableEntity
{
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float groundCheckRadius;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float slopeCheckDistance;
    [SerializeField]
    private float maxSlopeAngle;
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    private LayerMask hazardLayerMask;
    [SerializeField]
    private PhysicsMaterial2D noFriction, lowFriction, fullFriction;
    [SerializeField]
    private float maxMovementForce = 30f;
    [SerializeField]
    private float maxStoppingForce = 50f;
    [SerializeField]
    private float carryingExtraForcePerMass = 3f;

    [SerializeField]
    private Animator animator;
    public Animator Animator {get{return animator;}}

    [SerializeField]
    private Interaction interaction;
    public Interaction Interaction {get{return interaction;}}

    [SerializeField]
    private GrabAndThrow grabBox;
    public GrabAndThrow GrabBox {get{return grabBox;}}

    [SerializeField] 
    private GameObject spriteObject;
    public AudioSource audioSource {get; private set;}

    [SerializeField]
    private float carrySpeedMultiplier = 0.333f;
    [SerializeField]
    private float carryJumpMultiplier = 0.25f;

    [SerializeField]
    private Menu pauseMenu;
    [SerializeField] private GameObject minecartSpriteObject;

    [SerializeField] public Headlight headlight;

    private CameraRoom currentRoom;
    private float xInput;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;

    private int facingDirection = 1;
    public int FacingDirection {get{return facingDirection;}}

    private bool isGrounded = true;
    public bool IsGrounded {get{return isGrounded;}}
    private bool isOnSlope;
    private bool isJumping;
    private bool canWalkOnSlope;
    private bool canJump;
    private bool jumpNextFixedUpdate;

    private Vector2 newVelocity;
    private Vector2 newForce;
    public Vector2 capsuleColliderSize {get; private set;}

    private Vector2 slopeNormalPerp;

    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    public CapsuleCollider2D capsuleCollider {get{return cc;}}

    private List<Collider2D> ignoreCollisionWhileDead;

    private Rigidbody2D currentMovingPlatform;

    private float initialDrag;

    private PlayerState playerState; 
    // While controlmode is not Normal, can be used to control the player during cutscenes, etc.
    public float autoXInput {get; private set;} = 0f;

    private float timeSinceLastGrounded;

    // Amount of time after walkng off a ledge that the player can jump.
    [SerializeField] private float maxCoyoteTime = 0.5f;


    public List<FollowingItem> followingItems {get; private set;}

    // Set to false the moment the user regains control. Prevents double inputs. Also will prevent opening the menu.
    private bool canInteractThisFrame = true;

    private enum PlayerState {
        NORMAL,
        DISABLED,
        DEAD
    }

    protected override void Awake() {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        capsuleColliderSize = cc.size;
        unstableLayerMask = 1 << LayerMask.NameToLayer("UnstableObject");
        initialDrag = rb.drag;
        followingItems = new List<FollowingItem>();
        ignoreCollisionWhileDead = new List<Collider2D>();
    }

    private void Update()
    {
        CheckInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        CheckGround();

        // If dead, do ot apply any player movement forces, player has gone limp
        if (playerState == PlayerState.DEAD) {
            return;
        }
        
        SlopeCheck();
        ApplyMovement();
    }

    private void CheckInput()
    {
        if(playerState == PlayerState.NORMAL){
            xInput = Input.GetAxisRaw("Horizontal");

            if (xInput == 1 && facingDirection == -1)
            {
                Flip();
            }
            else if (xInput == -1 && facingDirection == 1)
            {
                Flip();
            }

            if (Input.GetButtonDown("Jump")) // Z
            {
                QueueJump();
            }

            if (Input.GetButtonDown("Interact") && canInteractThisFrame) // X
            {
                Interact();
            }

            if (Input.GetButtonDown("Cancel") && canInteractThisFrame) // C
            {
                Cancel();
            }

            if (Input.GetButtonDown("Pause") && canInteractThisFrame) // P/esc
            {
                OpenPauseMenu();
            }

            if (Input.GetButtonDown("Respawn") && canInteractThisFrame) // R
            {
                if(currentRoom.CanRespawn) Die();
            }
        }
        else{
            xInput = autoXInput;
        }

        canInteractThisFrame = true;
    }

    private void UpdateAnimation()
    {
        animator.SetBool("walking", xInput != 0);
    }


    private bool preventPropFly = false;
    private int unstableLayerMask;

    private void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, whatIsGround);

        // Check all collisions. For each grabbable, if it cannot be currently picked up (released recently),
        // then player also cannot jump off of it
        bool groundedLastUpdate = isGrounded;
        isGrounded = false;
        currentMovingPlatform = null;
        foreach (Collider2D c in colliders) {
            if (!c.enabled) continue;

            if (Physics2D.GetIgnoreCollision(cc, c)) continue;

            // Check for rigidbody if current moving platform not found yet
            if (!currentMovingPlatform) {
                // If floor has a rigidbody, track it so that it can be snapped to
                Rigidbody2D rb = c.gameObject.GetComponent<Rigidbody2D>();
                if (rb) {
                    // Check that it's not in the unstable object layer, which cannot be trated as a moving platform
                    if (( unstableLayerMask & (1 << c.gameObject.layer)) == 0) {
                        currentMovingPlatform = rb;
                    }
                }
            }
            
            if (preventPropFly) {
                Grabbable grabbable = c.gameObject.GetComponent<Grabbable>();
                if (grabbable && !grabbable.CanBeJumpedOff()) {
                    continue;
                } else {
                    Ground(groundedLastUpdate);
                    break;
                }
            } else {
                Ground(groundedLastUpdate);
                break;
            }
        }

        if(rb.velocity.y <= 0.0f)
        {
            isJumping = false;
        }

        if(isGrounded && !isJumping) //  && slopeDownAngle <= maxSlopeAngle
        {
            canJump = true;
        } 
        if (!isGrounded) {
            timeSinceLastGrounded += Time.fixedDeltaTime;
        }
    }

    private void Ground(bool groundedLastUpdate) {
        if (!groundedLastUpdate) {
            SoundManager.PlaySound(audioSource, "land");
        }
        isGrounded = true;
        timeSinceLastGrounded = 0;
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)new Vector2(0.0f, capsuleColliderSize.y / 2);

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            float slopeFAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            if (slopeFAngle < 75f)
            {
                isOnSlope = true;

                slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            }
        }
        else if (slopeHitBack)
        {
            float slopeBackAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
            if(slopeBackAngle < 75f)
            {
                isOnSlope = true;

                slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
            }
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;            

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if(slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }                       

            lastSlopeAngle = slopeDownAngle;
           
            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);

        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (isOnSlope && canWalkOnSlope && xInput == 0.0f)
        {
            rb.sharedMaterial = fullFriction;
            cc.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
            cc.sharedMaterial = noFriction;
        }
    }
    private void Interact(){
        interaction.InteractNearest();
    }

    private void Cancel(){
        bool canceled = interaction.CancelNearest();
        // open pause menu if nothing else was canceled (disabled)
        // if (!canceled) OpenMenu();
    }

    private void OpenPauseMenu() {
        DisableControl();
        pauseMenu.Show();
    }

    private void QueueJump()
    {
        jumpNextFixedUpdate = true;
    }   

    private void Jump()
    {
        if (canJump && timeSinceLastGrounded < maxCoyoteTime) {
            canJump = false;
            isJumping = true;
            newVelocity.Set(rb.velocity.x, 0.0f);
            rb.velocity = newVelocity;

            float carryMass = 0;
            if (grabBox.BoxRb) carryMass += grabBox.BoxRb.mass;
            float jumpMultiplier = 1 / (1 + carryMass*carryJumpMultiplier);
            float force = jumpForce * jumpMultiplier * rb.mass;
            newForce.Set(0.0f, force);
            rb.AddForce(newForce, ForceMode2D.Impulse);

            SoundManager.PlaySound(audioSource, "jump");
        }
    }

    private void ApplyMovement()
    {
        float carryMass = 0;
        if (grabBox.BoxRb) carryMass += grabBox.BoxRb.mass;
        float speedMultiplier = 1 / (1 + carryMass*carrySpeedMultiplier);
        float moveSpeed = movementSpeed * speedMultiplier;

        if (isGrounded && !isOnSlope && !isJumping) //if not on slope
        {
            newVelocity.Set(moveSpeed * xInput, 0.0f);
        }
        else if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping) //If on slope
        {
            newVelocity.Set(moveSpeed * slopeNormalPerp.x * -xInput, moveSpeed * slopeNormalPerp.y * -xInput);
        }
        else if (!isGrounded) //If in air
        {
            newVelocity.Set(moveSpeed * xInput, rb.velocity.y);
        }

        Vector2 velocityChange = newVelocity - rb.velocity;

        // Move target velocity in same direction of platform velocity if on one
        if (currentMovingPlatform) {
            velocityChange += currentMovingPlatform.velocity;
        }

        // if (isOnSlope && isGrounded && xInput == 0 && Mathf.Abs(rb.velocity.magnitude) < 0.75f) {
        //     Debug.Log("slope standing");
        // } else 
        if (xInput == 0 || Mathf.Sign(xInput) != Mathf.Sign(rb.velocity.x)) {
            float stoppingForce = maxStoppingForce + carryingExtraForcePerMass * (maxStoppingForce/maxMovementForce)*(rb.mass-1);
                velocityChange = Vector2.ClampMagnitude(velocityChange, stoppingForce*Time.fixedDeltaTime);
                rb.AddForce(velocityChange, ForceMode2D.Impulse);
        } else {
            float movementForce = maxMovementForce + carryingExtraForcePerMass*(rb.mass-1);
            velocityChange = Vector2.ClampMagnitude(velocityChange, movementForce*Time.fixedDeltaTime);
            rb.AddForce(velocityChange, ForceMode2D.Impulse);
        }

        if (jumpNextFixedUpdate) {
            jumpNextFixedUpdate = false;
            Jump();
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        spriteObject.transform.Rotate(0.0f, 180.0f, 0.0f);
        if (followingItems.Count > 0) followingItems[0].followOffset = Vector2.right * facingDirection * -1.5f;
    }

    public override void Die() {
        if (playerState == PlayerState.DEAD) return;

        foreach (var other in ignoreCollisionWhileDead) {
            Physics2D.IgnoreCollision(cc, other, true);
        }

        ForceReleaseGrabbed();
        playerState = PlayerState.DEAD;
        animator.SetBool("dead", true);
        
        rb.drag = 1f;
        rb.gravityScale = 1.5f;
        rb.sharedMaterial = lowFriction;
        cc.sharedMaterial = lowFriction;
        currentRoom.VirtualCam.Follow = null; // prevent camera follow movement while dead

        SoundManager.PlaySound(audioSource, "death");

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay() {
        yield return new WaitForSeconds(currentRoom.GetRespawnDelay());
        Respawn();
    }

    public void Respawn()
    {
        ForceReleaseGrabbed();
        currentRoom.VirtualCam.Follow = transform;
        rb.velocity.Set(0.0f, 0.0f);
        MoveToRespawnPoint(currentRoom.CurrentSpawnPoint());
        currentRoom.RespawnItemsAfterDeath();

        rb.drag = initialDrag;
        rb.gravityScale = 1f;

        foreach (var other in ignoreCollisionWhileDead) {
            Physics2D.IgnoreCollision(cc, other, false);
        }

        animator.SetBool("dead", false);
        playerState = PlayerState.NORMAL;
    }

    void ForceReleaseGrabbed() {
        if(GrabBox.IsHoldingBox()){
            GrabBox.ReleaseGrabbed(throwBox: false, forced: true);
        }
    }

    public void EnableControl() {
        playerState = PlayerState.NORMAL;
        interaction.RefreshNearestInteractable();
        canInteractThisFrame = false;
    }

    public void DisableControl() {
        playerState = PlayerState.DISABLED;
        interaction.RefreshNearestInteractable();
    }

    public void EnablePhysics() {
        cc.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic; 
    }

    public void DisablePhysics() {
        cc.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void IgnoreCollisionWhileDead(Collider2D other) {
        ignoreCollisionWhileDead.Add(other);
    }

    public void PickupFollowingItem(FollowingItem item) {
        followingItems.Add(item);
        FollowingItemTrailUpdate();
    }

    public void FollowingItemTrailUpdate() {
        if (followingItems.Count == 0) return;

        followingItems[0].Follow(spriteObject.transform);
        followingItems[0].followOffset = Vector2.right * facingDirection * -1.5f;

        for (int i = 1; i < followingItems.Count; i++) {
            followingItems[i].Follow(followingItems[i-1]);
        }
    }

    public bool HasControl() {
        return playerState == PlayerState.NORMAL;
    }

    public bool IsDead() {
        return playerState == PlayerState.DEAD;
    }

    public CameraRoom GetCurrentRoom() {
        return currentRoom;
    }
    public void SetCameraRoom(CameraRoom room) {
        currentRoom = room;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        // If other object's layer is in hazards layermask, die
        if (hazardLayerMask == (hazardLayerMask | (1 << other.gameObject.layer))) {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // If other object's layer is in hazards layermask, die
        if (hazardLayerMask == (hazardLayerMask | (1 << other.gameObject.layer))) {
            Die();
        }
    }

    public LayerMask GetGroundLayerMask() {
        return whatIsGround;
    }

    public void MoveToRespawnPoint(RespawnPoint respawnPoint) {
        transform.position = respawnPoint.transform.position + Vector3.up * cc.size.y / 2f;
    }

    public void EnterMinecart() {
        minecartSpriteObject.SetActive(true);
        animator.SetBool("minecart", true);
    }

    public void ExitMinecart() {
        SetMinecartRotation(Vector2.up);
        minecartSpriteObject.SetActive(false);
        animator.SetBool("minecart", false);
    }

    public void SetMinecartRotation(Vector2 lookRotation) {
        minecartSpriteObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookRotation);
        lookRotation.Set(lookRotation.x, lookRotation.y*2f);
        spriteObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookRotation);
    }

    public void SetAutoXInput(float input) {
        autoXInput = input;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public override void OnHit(int dmg, DamageHurtbox hurtbox)
    {
        // no hp for player, just die
        Die();
    }
}