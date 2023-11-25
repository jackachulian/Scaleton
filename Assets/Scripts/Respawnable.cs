using UnityEngine;

public class Respawnable : MonoBehaviour
{
    bool initialStateSaved;
    private Vector3 rPos;
    private Quaternion rRotation;
    private float rAngle;
    private Rigidbody2D _rb;

    private RigidbodyType2D rbType;

    protected virtual void Awake()
    {
        if (initialStateSaved) return;
        
        _rb = GetComponent<Rigidbody2D>();
        if (_rb) {
            rPos = _rb.position;
            rAngle = _rb.rotation;
            rbType = _rb.bodyType;
        } else {
            rPos = transform.position;
            rRotation = transform.rotation;
        }

        initialStateSaved = true;
    }

    public virtual void Respawn(){
        if (!initialStateSaved) return;

        if (_rb) {
            _rb.position = rPos;
            _rb.rotation = rAngle;
            _rb.bodyType = rbType;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        } else {
            transform.position = rPos;
            transform.rotation = rRotation;
        }
    }

}
