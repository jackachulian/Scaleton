using UnityEngine;

public class LockBox : MonoBehaviour {
    PlayerController playerController;
    private void OnTriggerEnter2D(Collider2D other) {
        
        if (other.TryGetComponent(out playerController)) {
            if (playerController.followingItems.Count > 0) {
                var key = playerController.followingItems[0];
                playerController.followingItems.RemoveAt(0);
                playerController.FollowingItemTrailUpdate();
                key.Unlock(this);
            }
        }
    }
}