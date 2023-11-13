using System;
using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    [SerializeField] private Transform[] respawnPoints;

    [SerializeField] private bool canRespawn = true;
    public bool CanRespawn {get{return canRespawn;}}

    /// <summary>
    /// If true, objects will not be respawned when exiting from this room.
    /// </summary>
    [SerializeField] private bool isSubRoom = false;

    private Transform objectsTransform;

    private PolygonCollider2D polygonCollider;

    private bool usesConfiner;
    private Respawnable[] respawnables;

    // Upon entering a room, this is set to the nearest respawn point.
    // item in index 0 by default
    public Transform currentRespawnPoint { get; private set; }

    private PlayerController player;

    private void OnValidate() {
        if (!polygonCollider) polygonCollider = GetComponent<PolygonCollider2D>();
        if (!objectsTransform) objectsTransform = transform.Find("Objects");
        respawnables = objectsTransform.GetComponentsInChildren<Respawnable>();
        usesConfiner = virtualCam.GetComponent<CinemachineConfiner>() != null;
        player = GameObject.Find("Player").GetComponent<PlayerController>();
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

        if (canRespawn) currentRespawnPoint = respawnPoints[0];
    }

    private void OnTriggerEnter2D(Collider2D other) {
        virtualCam.enabled = true;
        virtualCam.MoveToTopOfPrioritySubqueue();

        if(canRespawn){
            // Respawn items and set them to their original position when re-entering this area from another room.
            // Don't respawn if the room the player was in is a sub-room, meaning it's a sub-room of this room.
            CameraRoom previousRoom = player.GetCurrentRoom();
            if (!previousRoom || !previousRoom.isSubRoom) RespawnItems();

            // Find closest spawn point and set that to the respawn point upon entering
            currentRespawnPoint = respawnPoints[0];
            float distance = Vector2.Distance(currentRespawnPoint.position, player.transform.position);
            for (int i=1; i<respawnPoints.Length; i++) {
                var respawnPoint = respawnPoints[i];
                float newDistance = Vector2.Distance(respawnPoint.position, player.transform.position);
                if (newDistance < distance) {
                    currentRespawnPoint = respawnPoint;
                    distance = newDistance;
                }
            }
        }   

        player.SetCameraRoom(this);
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

    public void SetRespawnPoint(Transform respawnPoint) {
        currentRespawnPoint = respawnPoint;
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