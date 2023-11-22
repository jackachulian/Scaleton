using UnityEngine;

public class Respawnable : MonoBehaviour
{
    private Vector3 rPos;
    private Quaternion rRotation;
    private Rigidbody2D rb;

    private RigidbodyType2D rbType;

    void Awake()
    {
        rPos = transform.position;
        rRotation = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
        if (rb) rbType = rb.bodyType;
    }

    public virtual void Respawn(){
        transform.position = rPos;
        transform.rotation = rRotation;

        if (rb) {
            rb.bodyType = rbType;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

}
