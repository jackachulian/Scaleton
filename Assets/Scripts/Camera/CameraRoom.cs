using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    private PolygonCollider2D polygonCollider;

    private void OnValidate() {
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private readonly Color roomBorderGizmoColor = new Color(0f, 0.5f, 1f, 0.5f);
    private void OnDrawGizmos() {
        Gizmos.color = roomBorderGizmoColor;
        for (int i = 0; i < polygonCollider.points.Length-1; i++) {
            Gizmos.DrawLine(
                transform.TransformPoint(polygonCollider.points[i]), 
                transform.TransformPoint(polygonCollider.points[i+1])
            );
        }
        Gizmos.DrawLine(
            transform.TransformPoint(polygonCollider.points[0]),
            transform.TransformPoint(polygonCollider.points[polygonCollider.points.Length-1]));
    }

    private void Start() {
        if (virtualCam.Follow == null) {
            virtualCam.Follow = GameObject.Find("Player").transform;
        }

        // Create a collider that will block StableObjects
        GameObject grabbableBlocker = new GameObject("GrabbableBlocker") { layer = LayerMask.NameToLayer("GrabbableBlocker") };
        grabbableBlocker.transform.SetParent(transform, false);
    
        for (int i = 0; i < polygonCollider.pathCount; i++) {
            EdgeCollider2D edge = grabbableBlocker.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = 0.025f;
            Vector2[] points = polygonCollider.GetPath(i);
            Array.Resize(ref points, points.Length+1);
            points[points.Length-1] = points[0];
            edge.points = points;
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