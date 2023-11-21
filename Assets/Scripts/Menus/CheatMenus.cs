using System;
using Cinemachine;
using UnityEngine;

public class CheatMenus : MonoBehaviour {
    [SerializeField] private MenuManager dialog;

    private CameraRoom[] cameraRooms;

    private void Start() {
        cameraRooms = FindObjectsByType<CameraRoom>(FindObjectsSortMode.None);
        Array.Sort(cameraRooms, (roomA, roomB) => roomA.name.CompareTo(roomB.name)); // sort in alphabetical
    }

    private void Update() {
        if (!dialog.CanOpenDialog()) return;

        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F1)) OpenFastTravelMenu();
    }

    private void OpenFastTravelMenu() {
        Menu menu = dialog.CreateDialogMenu();
        foreach (var room in cameraRooms) {
            dialog.AddButton(menu, room.name, () => TeleportToRoom(room));
        }
    }

    private void TeleportToRoom(CameraRoom room) {
        // if (dialog.player.GetCurrentRoom() == room) return;

        TransitionManager.Transition(() => {
            dialog.player.MoveToRespawnPoint(room.currentRespawnPoint);
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            room.VirtualCam.MoveToTopOfPrioritySubqueue();
            brain.ManualUpdate();
            brain.ActiveBlend = null;
        });
    }
}