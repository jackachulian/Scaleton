using System.Collections;
using UnityEngine;

public class FollowingItem : MonoBehaviour {
    Transform target;

    Vector3 targetPosition;

    Vector3 vel;

    [SerializeField] float followDistance = 1.5f;

    [SerializeField] float followTime = 0.3f;

    bool unlockingLock = false;

    public void Follow(Transform target) {
        this.target = target;
    }

    public void Update() {
        if (!target) return;
        targetPosition = target.position + target.right * -followDistance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, followTime);
    }

    // Player pickup
    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) {
            target = other.transform.GetChild(0); // will target the sprite which is flipped left/right, use to follow behind player
            gameObject.layer = LayerMask.NameToLayer("FollowingItem"); // will allow this to collide with radius surrounding lockboxes
            GetComponent<BoxCollider2D>().isTrigger = false;
            player.followingItems.Add(this);
        }
    }

    public void Unlock(LockBox lockBox) {
        if (unlockingLock) return;

        unlockingLock = true;
        followDistance = 0f;
        followTime *= 0.6f;
        Follow(lockBox.transform);
        StartCoroutine(UnlockAfterDelay(lockBox));
    }

    IEnumerator UnlockAfterDelay(LockBox lockBox) {
        yield return new WaitForSeconds(1f);
        Destroy(lockBox.gameObject);
        Destroy(gameObject);
    }
}