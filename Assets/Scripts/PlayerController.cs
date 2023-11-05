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
    private Animator animator;

    [SerializeField]
    private GrabBox grabBox;

    [SerializeField]
    private float carrySpeedMultiplier = 0.333f;
    [SerializeField]
    private float carryJumpMultiplier = 0.25f;

    private float xInput;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;

    private int facingDirection = 1;
    public int FacingDirection {get{return facingDirection;}}

    private bool isGrounded;
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
        xInput = Input.GetAxisRaw("Horizontal");

        if (xInput == 1 && facingDirection == -1)
        {
            Flip();
        }
        else if (xInput == -1 && facingDirection == 1)
        {
            Flip();
        }

        if (Input.GetButtonDown("Grab"))
        {
            Grab();
        }

        if (Input.GetButtonDown("Jump"))
        {
            QueueJump();
        }

    }

    private void UpdateAnimation()
    {
        animator.SetBool("walking", xInput != 0);
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if(rb.velocity.y <= 0.0f)
        {
            isJumping = false;
        }

        if(isGrounded && !isJumping) //  && slopeDownAngle <= maxSlopeAngle
        {
            canJump = true;
        }

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
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }
    private void Grab(){
        grabBox.GrabPressed();
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
            newVelocity.Set(0.0f, 0.0f);
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

        rb.velocity = newVelocity;

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}