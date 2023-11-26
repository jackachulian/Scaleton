using System;
using System.Collections;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraRoom : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    public CinemachineVirtualCamera VirtualCam {get{return virtualCam;}}

    private RespawnPoint[] respawnPoints;

    [SerializeField] private bool canRespawn = true;
    public bool CanRespawn {get{return canRespawn;}}

    // true while player is within this room
    private bool playerWithin = false;

    /// <summary>
    /// If true, objects will not be respawned when exiting from this room.
    /// </summary>
    [SerializeField] private bool isSubRoom = false;

    // Block grabbables from etnering and exiting this room.
    [SerializeField] private bool blockGrabbables = true;

    [SerializeField] private bool respawnObjectsOnDeath = true;

    [SerializeField] private float respawnDelay = 1.5f;

    [SerializeField] private RespawnType respawnType;
    public enum RespawnType {
        LastTouched,
        Nearest,
        FarthestFromBoss,
        First
    }

    [SerializeField] private float brightness = 0.5f;

    [SerializeField] private ParticleSystem ambientParticles;

    // controls the density of the ambient dust particles, average particles per tile (particles/unit^2)
    [SerializeField] private float particleDensity = 1f; 

    [SerializeField] private DamageableEntity boss;

    public Transform objectsTransform {get; private set;}

    private PolygonCollider2D polygonCollider;

    private bool usesConfiner;
    private Respawnable[] respawnables;

    // Upon entering a room, this is set to the nearest respawn point.
    // item in index 0 by default
    public RespawnPoint currentRespawnPoint { get; private set; }

    private PlayerController player;

    // Minimum time a player mst not be in this room in order to respawn its items when re-entering
    private static float minRespawnTime = 0.75f;
    private float exitTimer = 0f;

    private void OnValidate() {
        if (!polygonCollider) polygonCollider = GetComponent<PolygonCollider2D>();
        
        usesConfiner = virtualCam.GetComponent<CinemachineConfiner>() != null;

        RepositionAmbientParticles();
    }

    private void Awake() {
        respawnPoints = transform.GetComponentsInChildren<RespawnPoint>();
    }

    private void RepositionAmbientParticles() {
        if (!polygonCollider) return;
        var bounds = polygonCollider.bounds;
        
        ParticleSystem.ShapeModule shape = ambientParticles.shape;
        shape.position = transform.InverseTransformPoint(bounds.center);
        shape.scale = bounds.size;

        ParticleSystem.EmissionModule emission = ambientParticles.emission;
        float duration = ambientParticles.main.duration;
        float area = bounds.size.x * bounds.size.y;
        emission.rateOverTime = particleDensity * area / duration;

        // Debug.Log(gameObject + " calculated rate: "+emission.rateOverTime.constant);

        // particleCount = duration * rateOverTime;
        // density = particleCount / area
        // density = duration * rateOverTime / area;
        // density*area = duration*rateOverTime;
        /// density*area / duration = rateOverTime;
    }

    private void Start() {
        if (!objectsTransform) objectsTransform = transform.Find("Objects");
        respawnables = objectsTransform.GetComponentsInChildren<Respawnable>();

        if (!player) player = GameObject.Find("Player").GetComponent<PlayerController>();

        if (virtualCam.Follow == null) {
            virtualCam.Follow = player.transform;
        }

        RepositionAmbientParticles();
        ambientParticles.Stop();
        ambientParticles.Clear();

        if (blockGrabbables) {
            // Create a collider that will block boxes
            GameObject grabbableBlocker = new GameObject("GrabbableBlocker") { layer = LayerMask.NameToLayer("GrabbableBlocker") };
            grabbableBlocker.transform.SetParent(transform, false);
        
            if (polygonCollider) {
                for (int i = 0; i < polygonCollider.pathCount; i++) {
                    EdgeCollider2D edge = grabbableBlocker.AddComponent<EdgeCollider2D>();
                    edge.edgeRadius = 0.025f;
                    Vector2[] points = polygonCollider.GetPath(i);
                    Array.Resize(ref points, points.Length+1);
                    points[points.Length-1] = points[0];
                    edge.points = points;
                }
            }
        }
        
        currentRespawnPoint = respawnPoints[0];
    }

    private void Update() {
        if (!playerWithin && exitTimer > 0) {
            exitTimer -= Time.deltaTime;
            if (exitTimer <= 0) {
                exitTimer = 0f;
                ambientParticles.Stop();
                ambientParticles.Clear();
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
       if (!playerWithin) EnterRoom();
    }

    public void EnterRoom() {
        if (player.IsDead()) return;

        playerWithin = true;

        virtualCam.enabled = true;
        virtualCam.MoveToTopOfPrioritySubqueue();

        if (!ambientParticles.isPlaying) ambientParticles.Play();

        SetBrightness(brightness);

        if(canRespawn){
            CameraRoom previousRoom = player.GetCurrentRoom();
            if (previousRoom)
            if (!previousRoom || !previousRoom.isSubRoom) {
                // Respawn items and set them to their original position when re-entering this area from another room.
                // Don't respawn if the room the player was in is a sub-room, meaning it's a sub-room of this room.
                // Also, don't respawn if the player just exited this room very recently.
                if (exitTimer <= 0f) RespawnItems();

                // Find closest spawn point and set that to the respawn point upon entering
                currentRespawnPoint = respawnPoints[0];
                float distance = Vector2.Distance(currentRespawnPoint.transform.position, player.transform.position);
                for (int i=1; i<respawnPoints.Length; i++) {
                    var respawnPoint = respawnPoints[i];
                    float newDistance = Vector2.Distance(respawnPoint.transform.position, player.transform.position);
                    if (newDistance < distance) {
                        currentRespawnPoint = respawnPoint;
                        distance = newDistance;
                    }
                }
            }
        }   

        player.SetCameraRoom(this);
        exitTimer = 0f;
    }

    public void SetBrightness(float brightness) {
        GlobalLight.SetBrightness(brightness, 0.75f);
    }

    // private void OnTriggerStay2D(Collider2D other) {
    //     if (player.IsDead()) return;

    //     playerWithin = true;
        
    //     if(virtualCam.enabled == false){
    //         virtualCam.enabled = true;
    //         virtualCam.MoveToTopOfPrioritySubqueue();
    //         if(canRespawn){
    //             GameObject.Find("Player").GetComponent<PlayerController>().SetCameraRoom(this);
    //         }
    //     }
    // }

    private void OnTriggerExit2D(Collider2D other) {
        ExitRoom();
    }

    public void ExitRoom() {
        if (player.IsDead()) return;

        exitTimer = minRespawnTime;
        playerWithin = false;

        virtualCam.enabled = false;

    }

    public void RespawnItems(){
        foreach(Respawnable r in respawnables){
            r.Respawn();
        }
    }

    public void RespawnItemsAfterDeath() {
        if (respawnObjectsOnDeath) RespawnItems();
    }

    public void SetRespawnPoint(RespawnPoint respawnPoint) {
        if (respawnPoints.Contains(respawnPoint)) currentRespawnPoint = respawnPoint;
    }

    public void SetRespawnType(RespawnType type) {
        respawnType = type;
    }

    public RespawnPoint DefaultRespawnPoint() {
        if (respawnPoints.Length == 0) return null;
        return respawnPoints[0];
    }

    public RespawnPoint CurrentSpawnPoint() {
        if (respawnType == RespawnType.LastTouched) {
            return currentRespawnPoint;
        } else if (respawnType == RespawnType.Nearest) {
            return respawnPoints.OrderBy(point => Vector2.Distance(MenuManager.player.transform.position, point.transform.position)).FirstOrDefault();
        } else if (respawnType == RespawnType.FarthestFromBoss) {
            return respawnPoints.OrderBy(point => Vector2.Distance(boss.transform.position, player.transform.position)).FirstOrDefault();
        } else {
            return DefaultRespawnPoint();
        }
    }

    public float GetRespawnDelay() {
        return respawnDelay;
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