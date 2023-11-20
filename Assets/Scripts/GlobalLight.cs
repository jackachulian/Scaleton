using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour {
    private Light2D light2D;

    private static float targetBrightness;

    private static float vel;

    private void Awake() {
        light2D = GetComponent<Light2D>();
        targetBrightness = light2D.intensity;
    }

    public static void SetBrightness(float b) {
        targetBrightness = b;
    }

    private void Update() {
        light2D.intensity = Mathf.SmoothDamp(light2D.intensity, targetBrightness, ref vel, 0.75f);
    }
}