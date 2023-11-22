using UnityEngine;

public class MetalSheenShaderUpdater : MonoBehaviour {
    private Transform lightTransform;
    [SerializeField] private Material material;

    private void Start() {
        lightTransform = MenuManager.player.headlight.transform;
    }

    private void Update() {
        material.SetVector("_LightPosition", lightTransform.position);
    }
}