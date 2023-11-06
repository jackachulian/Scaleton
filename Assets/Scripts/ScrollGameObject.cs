using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollGameObject : MonoBehaviour
{
    public Scroll scroll;

   void OnTriggerEnter2D(Collider2D c){
    if(c.gameObject.GetComponent<PlayerController>() != null){
        // <add to inventory here>
        Debug.Log("Player picked up " + scroll.title + "!");
        Destroy(gameObject);
    }
    else{
        Debug.Log("NOT player entered scroll! ============================================================================================================================");
    }
   }

}
