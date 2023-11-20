using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CheatMenus : MonoBehaviour {
    [SerializeField] private DialogMenuManager dialog;

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
        TransitionManager.Transition(() => {
            dialog.player.MoveToRespawnPoint(room.currentRespawnPoint);
            // todo: find some way to prevent the blend between cameras, if possible
        });
    }
}