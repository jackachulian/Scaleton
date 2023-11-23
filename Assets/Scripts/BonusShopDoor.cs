using UnityEngine;

public class BonusShopDoor : MonoBehaviour {
    PlayerController playerController;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent(out playerController)) {
            bool foundE,foundW,foundS;
            foundE = foundW = foundS = false;
            foreach(InventoryItem i in SaveData.inventory){
                if(i.title == "Eastern Fragment"){
                    foundE = true;
                }
                else if(i.title == "Western Fragment"){
                    foundW = true;
                }
                else if(i.title == "Southern Fragment"){
                    foundS = true;
                }
            }
            if (foundE && foundW && foundS) {
                this.gameObject.transform.GetChild(0).GetComponent<Animator>().SetBool("Closed",true);
                this.gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
}