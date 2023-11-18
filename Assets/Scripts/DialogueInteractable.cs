using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : Interactable
{
    [SerializeField]
    private GameObject notification;
    [SerializeField]
    private string[] dialogue;
    [SerializeField]
    private Dialogue dialogueBox;

    private void Awake() {
        if (!dialogueBox) {
            dialogueBox = GameObject.Find("Canvas").transform.Find("Dialogue Box").GetComponent<Dialogue>();
        }
    }

    public override void Interact()
    {
        dialogueBox.StartDialogue(dialogue);
    }

    public override void Hover() {
        notification.SetActive(true);
    }

    public override void Unhover() {
        notification.SetActive(false);
    }
}
