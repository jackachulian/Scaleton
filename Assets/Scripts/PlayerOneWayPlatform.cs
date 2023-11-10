using System.Collections;
using UnityEngine;

public class PlayerOneWayPlatform : MonoBehaviour
{
    private PlatformEffector2D currentOneWayPlatform;

    PlayerController playerController;


    private void Awake() {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {   
        // cannot fall through platforms while holding a box
        if (playerController.GrabBox.IsHoldingBox()) return;

        if (Input.GetAxisRaw("Vertical") < -0.5f)
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject.GetComponent<PlatformEffector2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(playerController.capsuleCollider, platformCollider, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(playerController.capsuleCollider, platformCollider, false);
    }
}