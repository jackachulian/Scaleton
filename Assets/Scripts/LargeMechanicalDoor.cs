using UnityEngine;

public class LargeMechanicalDoor : Respawnable {
    [SerializeField] bool opened;
    [SerializeField] float closeSpeed = 12f;
    [SerializeField] float openSpeed = 4f;
    Vector3 closedPosition;
    Vector3 targetPosition;
    bool doorMoving;
    private static float height = 5f;

    bool initialState; // true = initially open, false = initally closed

    protected override void Awake() {
        base.Awake();
        initialState = opened;
        closedPosition = transform.position;
        if (opened) transform.position = transform.position + Vector3.up * height;
    }

    private void Update() {
        if (doorMoving) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, (opened ? openSpeed : closeSpeed)*Time.deltaTime);
            if (transform.position == targetPosition) {
                doorMoving = false;
            }
        }
    }

    public void Close() {
        targetPosition = closedPosition;
        opened = false;
        doorMoving = true;
    }

    public void Open() {
        targetPosition = closedPosition + Vector3.up * height;
        opened = true;
        doorMoving = true;
    }

    public override void Respawn()
    {
        opened = initialState;
        if (opened) {
            transform.position = closedPosition + Vector3.up * height;
        } else {
            transform.position = closedPosition;
        }
    }
}