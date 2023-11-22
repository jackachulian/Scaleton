using UnityEngine;

public class DialogueInteractable : Interactable
{
    [SerializeField]
    private string[] dialogue;

    public override void Interact()
    {
        MenuManager.globalDialogue.StartDialogue(dialogue);
    }
}
