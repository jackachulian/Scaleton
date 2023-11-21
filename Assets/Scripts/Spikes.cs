using UnityEngine;

public class spikes : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.GetComponent<PlayerController>()){
            c.gameObject.GetComponent<PlayerController>().Respawn();
        }
        else{
            print("spikes collided non-player");
        }
    }
}
