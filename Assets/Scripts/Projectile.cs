using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField] private GameObject hitEffect;

    private void OnCollisionEnter2D(Collision2D other) {
        Instantiate(hitEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}