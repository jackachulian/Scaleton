using UnityEngine;

public class GrabAndThrow : MonoBehaviour
{
    private Grabbable grabbedBox;

    private Rigidbody2D playerRb;
    private Rigidbody2D boxRb;
    public Rigidbody2D BoxRb {get{return boxRb;}}

    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject grabPoint;

    [SerializeField] private Vector2 dropCheckOffset = new Vector2(-0.25f, 0);
    [SerializeField] private Vector2 dropCheckSize = new Vector2(0.5f, 0.5f);

    [SerializeField] private Vector2 forwardThrowForce = new Vector2(6f, 4f);
    [SerializeField] private Vector2 upwardThrowForce = new Vector2(0.5f, 7f);
    [SerializeField] private Vector2 downwardThrowForce = new Vector2(0.25f, -7.1f);

    [SerializeField] private float throwingExtraForcePerMass = 3f;

    [SerializeField] private LayerMask obstructionLayerMask;

    private void Awake() {
        playerRb = playerController.GetComponent<Rigidbody2D>();
    }

    public void Grab(Grabbable grabbable){
        grabbedBox = grabbable;
        grabbedBox.DetachFromRoboticHand();
        boxRb = grabbedBox.GetComponent<Rigidbody2D>();
        boxRb.isKinematic = true;
        grabbedBox.GetComponent<Collider2D>().enabled = false;

        // Conservation of momentum
        Vector2 initialPlayerMomentum = playerRb.velocity * playerRb.mass;
        playerRb.mass += boxRb.mass;
        playerRb.velocity = initialPlayerMomentum / playerRb.mass;

        grabbedBox.transform.parent = grabPoint.transform;
        grabbedBox.transform.localPosition = Vector2.zero;
        grabbedBox.transform.localRotation = Quaternion.identity;

        playerController.Animator.SetBool("carrying", true);

        playerController.gameObject.layer = LayerMask.NameToLayer("PlayerHoldingGrabbable");

        SoundManager.PlaySound(playerController.audioSource, "grab");

        Debug.Log("Grab!");
    }

    // Attempt to let go of the box.
    // If grab point is obstructed by terrain, cannot drop here
    public void ReleaseGrabbed(bool throwBox, bool forced = false) {
        Collider2D obstruction = Physics2D.OverlapBox(grabPoint.transform.position + (Vector3)dropCheckOffset*playerController.FacingDirection, dropCheckSize, 0f, obstructionLayerMask);

        Debug.Log(obstruction);
        if (obstruction && !forced) Debug.Log(LayerMask.LayerToName(obstruction.gameObject.layer));

        if (obstruction == null || forced) {
            boxRb.isKinematic = false;
            var bc = grabbedBox.GetComponent<Collider2D>();
            bc.enabled = true;
            // GetComponent<Interaction>().AddInteractable(grabbedBox);

            playerRb.mass -= boxRb.mass;

            grabbedBox.transform.parent = null;
            grabbedBox.transform.rotation = Quaternion.identity;

            boxRb.velocity = Vector2.zero;
            
            if (throwBox) {
                float yInput = Input.GetAxisRaw("Vertical");
                Vector2 force;
                if (yInput > 0.7f) {
                    force = upwardThrowForce;
                } else if (yInput < -0.7f && !playerController.IsGrounded) {
                    force = downwardThrowForce;
                } else {
                    force = forwardThrowForce;
                }

                Vector2 throwForce = new Vector2(force.x*playerController.FacingDirection, force.y);

                float forceMagnitude = throwForce.magnitude;
                // add extra force - use 1 as the default throw and extra is beyond 1 mass
                float extraForce = (boxRb.mass-1) * throwingExtraForcePerMass;
                throwForce = throwForce.normalized * (forceMagnitude + extraForce);

                boxRb.AddForce(throwForce, ForceMode2D.Impulse);

                SoundManager.PlaySound(playerController.audioSource, "throw");
            } else {
                SoundManager.PlaySound(playerController.audioSource, "drop");
            }

            playerController.Animator.SetBool("carrying", false);

            playerController.gameObject.layer = LayerMask.NameToLayer("Player");

            grabbedBox.Release();



            grabbedBox = null;
            boxRb = null;
            if(forced){
                Debug.Log("Box forcefully dropped");
            }
            else{
                Debug.Log("Box dropped");
            }
        } else {
            Debug.Log("Can't drop here!");
        }
    }

    public bool IsHoldingBox()
    {
        return grabbedBox != null;
    }

    public Grabbable GetHeldBox()
    {
        return grabbedBox;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(grabPoint.transform.position + (Vector3)dropCheckOffset*playerController.FacingDirection, dropCheckSize);
    }
}
