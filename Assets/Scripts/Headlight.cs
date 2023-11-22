using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Headlight : MonoBehaviour {
    [SerializeField] private Light2D light2D;

    private Color fadingFrom;
    private Color fadingTo;

    private readonly static float fadeTime = 4f;

    private float delta = 1f;

    private void Awake() {
        delta = 1f;
    }

    private void Update() {
        if (delta < 1f) {
            delta += Time.deltaTime/fadeTime;
            if (delta > 1f) delta = 1f;
            light2D.color = Color.Lerp(fadingFrom, fadingTo, delta);
        }
    }

    public void SetColor(Color color) {
        fadingFrom = light2D.color;
        fadingTo = color;
        delta = 1 - delta;
    }
}