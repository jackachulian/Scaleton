using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacguffinGameObject : MonoBehaviour
{
    public Macguffin m;
    [SerializeField] Dialogue dialogueBox;

   void OnTriggerEnter2D(Collider2D c){
    if(c.gameObject.GetComponent<PlayerController>() != null){
        SaveData.inventory.Add(m);
        Debug.Log("Player picked up " + m.title + "!");
        Destroy(gameObject);
        dialogueBox.StartDialogue(m.collectionMessage);
    }
    else{
        Debug.Log("NOT player entered macguffin! ============================================================================================================================");
    }
   }

}
