using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class Cutscene : MonoBehaviour {
    public static Cutscene current {get; private set;}

    public string[] dialogue;

    [SerializeField] private LargeMechanicalDoor door; 

    [SerializeField] private Transform[] waypoints;

    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;

    public void StartCutscene() {
        if (current != null) {
            Debug.LogError("Trying to start a cutscene while one is already active");
            return;
        }
        current = this;
        MenuManager.globalDialogue.StartDialogue(dialogue);
    }

    // Cutscnees will be on the RoomBorder layers so only players should be able to trigger
    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log(other + "entered cutscene trigger");
        gameObject.SetActive(false);
        StartCutscene();
    }

    // Return true if the dialogue calling this function is ready to display the next line,
    // or false if it should wait for another coroutine to start the next line.
    public bool ParseCommand(string cmd, string[] args) {
        // used by pre-boss cutscene
        if (cmd == "closedoor") {
            door.Close();
        }

        // used by post-boss cutscene
        else if (cmd == "opendoor") {
            door.Open();
        }

        else if (cmd == "moveplayertowaypoint") {
            bool waitForPlayer = args.Length <= 2 || args[2] == "waitforplayer"; // default: true

            var waypoint = waypoints[int.Parse(args[1])];
            MenuManager.globalDialogue.StartCoroutine(MovePlayerToXPosition(waypoint.position.x, waitForPlayer));
            return !waitForPlayer;
        }

        // index -1 will be the current room's default camera
        else if (cmd == "changevirtualcamera") {
            int camIndex = int.Parse(args[1]);
            if (camIndex == -1) {
                MenuManager.player.GetCurrentRoom().VirtualCam.MoveToTopOfPrioritySubqueue();
            } else {
                virtualCameras[camIndex].MoveToTopOfPrioritySubqueue();
            }
        }

        else {
            Debug.LogError("Unknown cutscene cmd: \""+cmd+"\"");
        }

        return true;
    }

    IEnumerator MovePlayerToXPosition(float targetX, bool nextLineAfter) {
        MenuManager.player.SetAutoXInput(Mathf.Sign(targetX - MenuManager.player.transform.position.x));
        while (Math.Abs(MenuManager.player.transform.position.x - targetX) > 0.2f) {
            yield return null;
        }
        MenuManager.player.SetAutoXInput(0);
        if (nextLineAfter) MenuManager.globalDialogue.NextLine();
    }
}