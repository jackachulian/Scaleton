using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    private void Start() {
        if (virtualCam.Follow == null) {
            virtualCam.Follow = GameObject.Find("Player").transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        virtualCam.enabled = true;
        virtualCam.MoveToTopOfPrioritySubqueue();
    }

    private void OnTriggerExit2D(Collider2D other) {
        // virtualCam.enabled = false;
    }
}