using UnityEngine;

public class ChangingDialogueInteractable : Interactable
{
    [SerializeField] NpcDialogue[] dialogue;
    private int dialogueIndex;

    public void Start(){
        dialogueIndex = dialogue.Length-1;
    }

    public override void Interact()
    {
        if(dialogueIndex >= dialogue.Length-1){
            dialogueIndex = 0;
        }
        else{
            dialogueIndex++;
        }
        Debug.Log(dialogueIndex);
        MenuManager.globalDialogue.StartDialogue(dialogue[dialogueIndex].lines);
    }
}
