using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : Menu {
    public Button inventoryItemPrefab;

    [SerializeField] private Dialogue dialogue;

    public override void Show() {
        base.Show();
        PopulateInventoryItems();
        SelectFirstItemNextFrame();
    }

    public void PopulateInventoryItems() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        if (SaveData.inventory.Count == 0) {
            Button button = Instantiate(inventoryItemPrefab, transform);
            button.transform.GetChild(0).GetComponent<TMP_Text>().text = "<No items>";
            button.interactable = false;
        }

        foreach (InventoryItem item in SaveData.inventory) {
            Button button = Instantiate(inventoryItemPrefab, transform);
            button.transform.GetChild(0).GetComponent<TMP_Text>().text = item.name;
            button.onClick.AddListener(() => item.Use());
        }
    }
}