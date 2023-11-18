using UnityEngine;

public class LockBox : MonoBehaviour {
    PlayerController playerController;
    bool beingUnlocked = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if (beingUnlocked) return;
        if (other.TryGetComponent(out playerController)) {
            if (playerController.followingItems.Count > 0) {
                var key = playerController.followingItems[0];
                playerController.followingItems.RemoveAt(0);
                playerController.FollowingItemTrailUpdate();
                beingUnlocked = true;
                key.Unlock(this);
            }
        }
    }
}