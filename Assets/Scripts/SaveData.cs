using System.Collections.Generic;

public class SaveData {
    public static List<InventoryItem> inventory;

    static SaveData() {
        inventory = new List<InventoryItem>();
    }
}