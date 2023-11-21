using UnityEngine;

public class Respawnable : MonoBehaviour
{
    private Vector3 rPos;
    private Quaternion rRotation;
    private Rigidbody2D rb;

    void Awake()
    {
        rPos = transform.position;
        rRotation = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void Respawn(){
        transform.position = rPos;
        transform.rotation = rRotation;

        if (rb) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

}
