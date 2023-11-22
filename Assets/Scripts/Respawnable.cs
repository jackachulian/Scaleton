using UnityEngine;

public class Respawnable : MonoBehaviour
{
    bool initialStateSaved;
    private Vector3 rPos;
    private Quaternion rRotation;
    private float rAngle;
    private Rigidbody2D rb;

    private RigidbodyType2D rbType;

    void Awake()
    {
        if (initialStateSaved) return;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb) {
            rPos = rb.position;
            rAngle = rb.rotation;
            rbType = rb.bodyType;
        } else {
            rPos = transform.position;
            rRotation = transform.rotation;
        }

        initialStateSaved = true;
    }

    public virtual void Respawn(){
        Debug.Log("Respawning "+name);

        if (rb) {
            rb.position = rPos;
            rb.rotation = rAngle;
            rb.bodyType = rbType;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        } else {
            transform.position = rPos;
            transform.rotation = rRotation;
        }
    }

}
