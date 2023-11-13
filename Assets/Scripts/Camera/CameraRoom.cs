using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] public GameObject spawnPoint;
    [SerializeField] public bool canRespawn = true;
    [SerializeField] private Transform objectsTransform;

    private PolygonCollider2D polygonCollider;

    private bool usesConfiner;
    private Respawnable[] respawnables;

    private void OnValidate() {
        polygonCollider = GetComponent<PolygonCollider2D>();
        respawnables = objectsTransform.GetComponentsInChildren<Respawnable>();
        usesConfiner = virtualCam.GetComponent<CinemachineConfiner>() != null;
    }

    private void Start() {
        if (virtualCam.Follow == null) {
            virtualCam.Follow = GameObject.Find("Player").transform;
        }

        // Create a collider that will block boxes
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
        if(canRespawn){
            // Respawn items and set them to their original position when re-entering this area from another room.
            RespawnItems();
            GameObject.Find("Player").GetComponent<PlayerController>().SetCameraRoom(this);
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if(virtualCam.enabled == false){
            virtualCam.enabled = true;
            virtualCam.MoveToTopOfPrioritySubqueue();
            if(canRespawn){
            GameObject.Find("Player").GetComponent<PlayerController>().SetCameraRoom(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        virtualCam.enabled = false;
    }

    public void RespawnItems(){
        foreach(Respawnable r in respawnables){
            r.Respawn();
        }
    }


    private static readonly Color roomBorderGizmoColor = new Color(0f, 0.5f, 1f, 0.5f);
    private static readonly Vector2 unitScreenSize = new Vector2(24f, 13.5f);
    private void OnDrawGizmos() {
        Gizmos.color = roomBorderGizmoColor;

        if (usesConfiner) {
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

        else {
            Gizmos.DrawWireCube(virtualCam.transform.position, unitScreenSize);
        }
        
    }
}