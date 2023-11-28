using UnityEngine;

public class SaveDataLoader : MonoBehaviour {
    private void Awake() {
        // TODO: replace with loading inventory from playerprefs or a file or somethin
        SaveData.LoadInventory();
    }
}