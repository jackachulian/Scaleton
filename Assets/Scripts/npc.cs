using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc : MonoBehaviour
{
    [SerializeField]
    private GameObject notification;
    [SerializeField]
    private string[] dialogue;
    [SerializeField]
    private Dialogue dialogueBox;

    public void talk(){
        dialogueBox.StartDialogue(dialogue);
    }

    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.GetComponent<PlayerController>()){
            Debug.Log("Player entered NPC range");
            notification.SetActive(true);
            c.gameObject.GetComponent<PlayerController>().interactibleNPC = this;
        }
        else{
            Debug.Log("Non-player entered NPC range");
        }
    }

    void OnTriggerExit2D(Collider2D c){
        if(c.gameObject.GetComponent<PlayerController>()){
            Debug.Log("Player exited NPC range");
            notification.SetActive(false);
            c.gameObject.GetComponent<PlayerController>().interactibleNPC = null;
        }
        else{
            Debug.Log("Non-player exited NPC range");
        }
    }


}
