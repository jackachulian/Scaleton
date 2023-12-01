using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleCamera : MonoBehaviour {
    [SerializeField] private Transform relativeHorizontal, // controls where player's x position is, raycast down from here to get y pos
    scrollStart, scrollEnd, minecartStop;

    private bool startPressed;

    private bool minecartStopPassed = false;

    private bool minecartStopped = false;

    private bool introSkipped = false;

    private static float cameraZOffset = -10f;

    [SerializeField] private float scrollSpeed = 7.5f;

    [SerializeField] private float stopDecel = -3f;

    private CameraRoom startingRoom;

    [SerializeField] private PlayerController player;

    [SerializeField] private AudioClip titleMusic;

    [SerializeField] private Transform cameraTransform;
    private CinemachineBrain cinemachineBrain;

    private void Awake() {
        // Load required scenes if this is the build version
        if (!Application.isEditor) {
            SceneManager.LoadScene("Dungeon", LoadSceneMode.Additive);
            SceneManager.LoadScene("Tech", LoadSceneMode.Additive);
            SceneManager.LoadScene("BonusShop", LoadSceneMode.Additive);
        }

        startPressed = false;
        player.DisableControl();
        player.DisablePhysics();
        player.EnterMinecart();

        SoundManager.Instance.PlayMusic(titleMusic, fade: false);
    }

    private void Start() {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        cinemachineBrain.enabled = false;

        cameraTransform = Camera.main.transform;

        startingRoom = GameObject.Find("A1").GetComponent<CameraRoom>();
        Transform titleCameraTransforms = GameObject.Find("TitleCameraTransforms").transform; // should be located in z1
        scrollStart = titleCameraTransforms.Find("ScrollStart");
        scrollEnd = titleCameraTransforms.Find("ScrollEnd");
        minecartStop = titleCameraTransforms.Find("MinecartStop");

        cameraTransform.position = scrollStart.position + Vector3.forward * cameraZOffset;
    }

    public void StartGame() {
        startPressed = true;

        startingRoom.VirtualCam.MoveToTopOfPrioritySubqueue();
    }

    private void Update() {
        // Pause to skip the intro
        if (startPressed && !introSkipped && Input.GetButtonDown("Pause")) SkipIntro();

        if (minecartStopped) return;

        // Decelerate if stopping point passed
        if (minecartStopPassed) scrollSpeed = Mathf.MoveTowards(scrollSpeed, 0, -stopDecel * Time.deltaTime);

        // If speed reaches 0, stop minecart, call a coroutine that waits a bit and then brings the player
        // out of the minecart, places them on the ground, and enable necessary gameplay components
        if (scrollSpeed == 0) {
            minecartStopped = true;
            StartCoroutine(ExitMinecartAfterDelay());
            return;
        }

        // Snap player to the ground under the relaative horizontal position transform which moves with the camera.
        var hit = Physics2D.Raycast(relativeHorizontal.position, Vector2.down, 24f, player.GetGroundLayerMask());
        if (hit) {
            player.transform.position = hit.point + Vector3.up * player.capsuleColliderSize*0.5f;
            player.SetMinecartRotation(hit.normal);
        }

        // Scroll forward
        cameraTransform.position = cameraTransform.position + (Vector3.right * scrollSpeed * Time.deltaTime);
        
        // If start button hasn't been pressed, loop back if past scrollEnd, maintaining a constant percieved speed
        if (!startPressed) {
            float xDiff = cameraTransform.position.x - scrollEnd.position.x;
            if (xDiff > 0) {
                cameraTransform.position = scrollStart.position + (xDiff * Vector3.right) + (Vector3.forward * cameraZOffset);
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

    IEnumerator ExitMinecartAfterDelay() {
        yield return new WaitForSeconds(0.75f);
        ExitMinecartAndEnablePlayer();
    }

    private void ExitMinecartAndEnablePlayer() {
        player.ExitMinecart();
        player.EnablePhysics();
        player.EnableControl();
        cinemachineBrain.enabled = true;
        enabled = false;
    }


    // skips the scrolling thing and places the player at the start - happens when pause is pressed during the intro
    private void SkipIntro() {
        TransitionManager.Transition(() => SpawnPlayerAtStart(), fadeColor: Color.black);
    }

    private void SpawnPlayerAtStart() {
        minecartStopped = true;
        ExitMinecartAndEnablePlayer();
        player.MoveToRespawnPoint(startingRoom.currentRespawnPoint);
    }
}