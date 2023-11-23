using UnityEngine;

public class ScaleStringLineFixer : MonoBehaviour {
    private LineRenderer lineRenderer;

    [SerializeField] private Transform point2Target;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update() {
        lineRenderer.SetPosition(1, transform.InverseTransformPoint(point2Target.position));
    }
}