using System.Collections;
using Cinemachine;
using UnityEngine;

public class Teleporter : MonoBehaviour {
    [SerializeField] private Transform destination;

    [SerializeField] private string destinationRoomName;


    private CameraRoom destinationRoom;

    private void Awake() {
        destinationRoom = GameObject.Find(destinationRoomName).GetComponent<CameraRoom>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.name != "Player") return;

        TransitionManager.Transition(() => {
            MenuManager.player.GetComponent<Rigidbody2D>().position = destination.position;
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            var blendTime = brain.m_DefaultBlend.m_Time;
            brain.m_DefaultBlend.m_Time = 0f;
            StartCoroutine(ResetBlendAfterShortDelay(brain, blendTime));
            destinationRoom.EnterRoom();
        }, Color.black);
    }

    IEnumerator ResetBlendAfterShortDelay(CinemachineBrain brain, float resetTime) {
        
        yield return new WaitForSeconds(0.5f);
        brain.m_DefaultBlend.m_Time = resetTime;
    }
}