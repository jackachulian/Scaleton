using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private PhysicsMaterial2D noFriction;
    [SerializeField]
    private PhysicsMaterial2D fullFriction;
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
    private float carrySpeedMultiplier = 0.333f;
    [SerializeField]
    private float carryJumpMultiplier = 0.25f;

    [SerializeField]
    private Menu pauseMenu;

    [SerializeField] public CameraRoom currentRoom;

    private float xInput;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;

    private int facingDirection = 1;
    public int FacingDirection {get{return facingDirection;}}

    private bool isGrounded;
    public bool IsGrounded {get{return isGrounded;}}
    private bool isOnSlope;
    private bool isJumping;
    private bool canWalkOnSlope;
    private bool canJump;
    private bool jumpNextFixedUpdate;

    private Vector2 newVelocity;
    private Vector2 newForce;
    private Vector2 capsuleColliderSize;

    private Vector2 slopeNormalPerp;

    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    public CapsuleCollider2D capsuleCollider {get{return cc;}}

    private Rigidbody2D currentMovingPlatform;

    private PlayerState playerState; 

    // Set to false the moment the user regains control. Prevents double inputs. Also will prevent opening the menu.
    private bool canInteractThisFrame = true;

    private enum PlayerState {
        NORMAL,
        DISABLED
    }

    private void Awake() {
        unstableLayerMask = 1 << LayerMask.NameToLayer("UnstableObject");
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();

        capsuleColliderSize = cc.size;
    }

    private void Update()
    {
        CheckInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        CheckGround();
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
                OpenMenu();
            }

            if (Input.GetButtonDown("Respawn") && canInteractThisFrame) // P/esc
            {
                Respawn();
            }
        }
        else{
            xInput = 0;
        }

        canInteractThisFrame = true;
    }

    private void UpdateAnimation()
    {
        animator.SetBool("walking", xInput != 0);
    }

    private int unstableLayerMask;
    private void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, whatIsGround);

        // Check all collisions. For each grabbable, if it cannot be currently picked up (released recently),
        // then player also cannot jump off of it
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
            
            Grabbable grabbable = c.gameObject.GetComponent<Grabbable>();
            if (grabbable && !grabbable.CanBeJumpedOff()) {
                continue;
            } else {
                isGrounded = true;
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
        // else if (!isGrounded) {
        //     canJump = false;
        // }
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
        // open pause menu if nothing else was canceled
        if (!canceled) OpenMenu();
    }

    private void OpenMenu() {
        DisableControl();
        pauseMenu.Show();
    }

    private void QueueJump()
    {
        jumpNextFixedUpdate = true;
    }   

    private void Jump()
    {
        if (canJump) {
            canJump = false;
            isJumping = true;
            newVelocity.Set(rb.velocity.x, 0.0f);
            rb.velocity = newVelocity;

            float carryMass = 0;
            if (grabBox.BoxRb) carryMass += grabBox.BoxRb.mass;
            float jumpMultiplier = 1 / (1 + carryMass*carryJumpMultiplier);
            Debug.Log("jump mult: "+jumpMultiplier);
            float force = jumpForce * jumpMultiplier * rb.mass;
            newForce.Set(0.0f, force);
            rb.AddForce(newForce, ForceMode2D.Impulse);
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
                velocityChange = Vector2.ClampMagnitude(velocityChange, stoppingForce*Time.deltaTime);
                rb.AddForce(velocityChange, ForceMode2D.Impulse);
        } else {
            float movementForce = maxMovementForce + carryingExtraForcePerMass*(rb.mass-1);
            velocityChange = Vector2.ClampMagnitude(velocityChange, movementForce*Time.deltaTime);
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
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void Respawn()
    {
        if(currentRoom.canRespawn){
            if(GrabBox.IsHoldingBox()){
                GrabBox.ReleaseGrabbed(GrabBox.currentlyGrabbed(),true);
            }
            newVelocity.Set(0.0f, 0.0f);
            rb.velocity = newVelocity;
            transform.position = currentRoom.spawnPoint.transform.position;
            currentRoom.respawnItems();
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

    public bool HasControl() {
        return playerState == PlayerState.NORMAL;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}