using System.Collections;
using UnityEngine;

public class FollowingItem : MonoBehaviour {
    Transform target;
    FollowingItem followingItemTarget;

    Vector3 targetPosition;

    Vector3 vel;

    [SerializeField] public Vector3 followOffset = new Vector3(-1.5f, 0, 0);

    [SerializeField] float followTime = 0.3f;

    bool unlockingLock = false;

    public void Follow(Transform target) {
        this.followingItemTarget = null;
        this.target = target;
    }

    public void Follow(FollowingItem target) {
        this.followingItemTarget = target;
        this.target = null;
    }

    public void Update() {
        if (followingItemTarget) {
            followOffset = followingItemTarget.followOffset;
            targetPosition = followingItemTarget.transform.position + followOffset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, followTime);
        }

        else if (target) {
            targetPosition = target.position + followOffset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, followTime);
        }
        
    }

    // Player pickup
    private void OnTriggerEnter2D(Collider2D other) {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) {
            target = other.transform.GetChild(0); // will target the sprite which is flipped left/right, use to follow behind player
            gameObject.layer = LayerMask.NameToLayer("FollowingItem"); // will allow this to collide with radius surrounding lockboxes
            GetComponent<BoxCollider2D>().isTrigger = false;
            player.followingItems.Add(this);
            player.FollowingItemTrailUpdate();
        }
    }

    public void Unlock(LockBox lockBox) {
        if (unlockingLock) return;
        followOffset = Vector2.zero;
        unlockingLock = true;
        followTime *= 0.7f;
        Follow(lockBox.transform);
        StartCoroutine(UnlockAfterDelay(lockBox));
    }

    IEnumerator UnlockAfterDelay(LockBox lockBox) {
        yield return new WaitForSeconds(1f);
        Destroy(lockBox.gameObject);
        Destroy(gameObject);
    }
}