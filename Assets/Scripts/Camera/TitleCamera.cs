using System.Collections;
using Cinemachine;
using UnityEngine;

public class TitleCamera : MonoBehaviour {
    [SerializeField] private Transform relativeHorizontal, // controls where player's x position is, raycast down from here to get y pos
    scrollStart, scrollEnd, minecartStop;

    private bool startPressed;

    private bool minecartStopPassed = false;

    private bool minecartStopped = false;

    private static float cameraZOffset = -10f;

    [SerializeField] private float scrollSpeed = 7.5f;

    [SerializeField] private float stopDecel = -3f;

    [SerializeField] private CameraRoom startingRoom;

    [SerializeField] private PlayerController player;

    [SerializeField] private CinemachineBrain cinemachineBrain; // to re-enable after the title scroll stops.

    private void Awake() {
        startPressed = false;
        player.DisableControl();
        player.DisablePhysics();
        player.EnterMinecart();

        cinemachineBrain.enabled = false;

        transform.position = scrollStart.position + Vector3.forward * cameraZOffset;
    }

    public void StartGame() {
        startPressed = true;

        startingRoom.VirtualCam.MoveToTopOfPrioritySubqueue();
    }

    private void Update() {
        // TEMPORARY
        if (Input.GetKeyDown(KeyCode.S)) startPressed = true;

        if (minecartStopped) return;

        // Decelerate if stopping point passed
        if (minecartStopPassed) scrollSpeed = Mathf.MoveTowards(scrollSpeed, 0, -stopDecel * Time.deltaTime);

        // If speed reaches 0, stop minecart, call a coroutine that waits a bit and then brings the player
        // out of the minecart, places them on the ground, and enable necessary gameplay components
        if (scrollSpeed == 0) {
            minecartStopped = true;
            StartCoroutine(ExitMinecart());
            return;
        }

        // Snap player to the ground under the relaative horizontal position transform which moves with the camera.
        var hit = Physics2D.Raycast(relativeHorizontal.position, Vector2.down, 24f, player.GetGroundLayerMask());
        if (hit) {
            player.transform.position = hit.point + Vector3.up * player.capsuleColliderSize*0.5f;
            player.SetMinecartRotation(hit.normal);
        }

        // Scroll forward, only if minecart hasn't reached the stopping point
        transform.position = transform.position + (Vector3.right * scrollSpeed * Time.deltaTime);
        
        // If game hasn't started, loop back if past scrollEnd, maintaining a constant percieved speed
        if (!startPressed) {
            float xDiff = transform.position.x - scrollEnd.position.x;
            if (xDiff > 0) {
                transform.position = scrollStart.position + (xDiff * Vector3.right) + (Vector3.forward * cameraZOffset);
            }
        }

        // if game has started, allow camera to move the player into the first room (A1).
        // keep moving forawrd untl minecartStop transform is passed
        else {
            float xDiff = player.transform.position.x - minecartStop.position.x;
            if (xDiff > 0) {
                minecartStopPassed = true;
            }
        }
    }

    IEnumerator ExitMinecart() {
        yield return new WaitForSeconds(0.75f);
        Debug.Log("Title screen minecart exited");
        player.ExitMinecart();
        player.EnablePhysics();
        player.EnableControl();
        cinemachineBrain.enabled = true;
        enabled = false;
    }
}