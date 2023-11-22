using UnityEngine;

public class Cutscene : MonoBehaviour {
    public static Cutscene current {get; private set;}

    public string[] dialogue;

    [SerializeField] private LargeMechanicalDoor door; 

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

    public void ParseCommand(string cmd) {
        // used by pre-boss cutscene
        if (cmd == "closedoor") {
            door.Close();
        }

        // used by post-boss cutscene
        else if (cmd == "opendoor") {
            door.Open();
        }

        else {
            Debug.LogError("Unknown cutscene cmd: \""+cmd+"\"");
        }
    }
}