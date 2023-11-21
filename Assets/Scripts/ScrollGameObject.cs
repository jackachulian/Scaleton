using UnityEngine;

public class ScrollGameObject : MonoBehaviour
{
    public Scroll scroll;

    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.GetComponent<PlayerController>() != null){
            SaveData.inventory.Add(scroll);
            Debug.Log("Player picked up " + scroll.title + "!");
            Destroy(gameObject);
        }
        else{
            Debug.Log("NOT player entered scroll!");
        }
    }

}
