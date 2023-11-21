using UnityEngine;

public class IntangibleFade : MonoBehaviour
{
    private Material material;
    
    private float t;
    [SerializeField] private float delayTime = 5f;

    [SerializeField] private float fadeTime = 5f;

    private void Start() {
        material = GetComponent<SpriteRenderer>().material;
    }

    private void Update() {
        t += Time.deltaTime;

        if (t > fadeTime+delayTime) Destroy(gameObject);

        else if (t > delayTime) {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, (t - delayTime)/fadeTime));
            material.color = newColor;
        }
    }
}
