using System;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private bool canRespawn = true;

    private void Start() {
        if (virtualCam.Follow == null) {
            virtualCam.Follow = GameObject.Find("Player").transform;
        }

        // Create a collider that will block StableObjects
        GameObject grabbableBlocker = new GameObject("GrabbableBlocker") { layer = LayerMask.NameToLayer("GrabbableBlocker") };
        grabbableBlocker.transform.SetParent(transform, false);

        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
    
        for (int i = 0; i < collider.pathCount; i++) {
            EdgeCollider2D edge = grabbableBlocker.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.025f;
            Vector2[] points = collider.GetPath(i);
            Array.Resize(ref points, points.Length+1);
            points[points.Length-1] = points[0];
            edge.points = points;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        virtualCam.enabled = true;
        virtualCam.MoveToTopOfPrioritySubqueue();
        if(canRespawn){
            GameObject.Find("Player").GetComponent<PlayerController>().respawnPoint = spawnPoint;
        }
    }

    private void OnTriggerStay(Collider2D other) {
        if(virtualCam.enabled == false){
            virtualCam.enabled = true;
            virtualCam.MoveToTopOfPrioritySubqueue();
            if(canRespawn){
                GameObject.Find("Player").GetComponent<PlayerController>().respawnPoint = spawnPoint;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        virtualCam.enabled = false;
    }
}