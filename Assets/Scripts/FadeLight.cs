using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FadeLight : MonoBehaviour {
    [SerializeField] private Light2D light2D;
    [SerializeField] private Color targetColor;

    [SerializeField] private float targetIntensity;

    [SerializeField] private float transitionTime = 0.75f;

    [SerializeField] private AnimationCurve curve;

    private Color previousColor;
    private float previousIntensity;

    private float delta = 0;

    private void Awake() {
        previousColor = light2D.color;
        previousIntensity = light2D.intensity;
    }

    private void Update() {
        delta += Time.deltaTime;
        UpdateColor();
    }

    private void UpdateColor() {
        light2D.color = Color.Lerp(previousColor, targetColor, delta/transitionTime);
        light2D.intensity = Mathf.Lerp(previousIntensity, targetIntensity, delta/transitionTime);
    }
}