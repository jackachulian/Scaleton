using UnityEngine;

public class LockBox : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        FollowingKey key;
        if (other.TryGetComponent(out key)) {
            key.Unlock(this);
        }
    }
}