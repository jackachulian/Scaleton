using UnityEngine;

public class DialogueInteractable : Interactable
{
    [SerializeField]
    private GameObject notification;
    [SerializeField]
    private string[] dialogue;


    public override void Interact()
    {
        MenuManager.globalDialogue.StartDialogue(dialogue);
    }

    public override void Hover() {
        notification.SetActive(true);
    }

    public override void Unhover() {
        notification.SetActive(false);
    }
}
