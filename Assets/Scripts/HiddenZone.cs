using UnityEngine;

public class HiddenZone : MonoBehaviour {
    float alpha = 1f;
    float alphaTarget = 1f;

    [SerializeField] Renderer coverTilesRenderer;

    private void Start() {
        coverTilesRenderer.material.color = new Color(1f, 1f, 1f, alpha);
    }

    private void Update() {
        if (alphaTarget != alpha) {
            alpha = Mathf.MoveTowards(alpha, alphaTarget, 4f*Time.deltaTime);
            coverTilesRenderer.sharedMaterial.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        alphaTarget = 0f;
    }

    private void OnTriggerExit2D(Collider2D other) {
        alphaTarget = 1f;
    }
}