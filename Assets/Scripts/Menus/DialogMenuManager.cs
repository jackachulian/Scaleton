using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogMenuManager : MonoBehaviour {

    [SerializeField] private Menu emptyMenuPrefab;

    [SerializeField] private Button choiceButtonPrefab;

    public PlayerController player {get; private set;}

    private void Start() {
        player = FindObjectOfType<PlayerController>();
    }

    public Menu CreateDialogMenu() {
        if (!CanOpenDialog()) {
            Debug.LogWarning("Dialog opened while player has control, or at a time dialog shouldn't be openable");
        }
        Menu menu = Instantiate(emptyMenuPrefab, transform);
        foreach (Transform child in menu.transform) {
            Destroy(child.gameObject);
        }
        player.DisableControl();
        return menu;
    }

    public void AddButton(Menu menu, string label, Action onPress) {
        Button button = Instantiate(choiceButtonPrefab, menu.transform);
        button.transform.GetChild(0).GetComponent<TMP_Text>().text = label;
        button.onClick.AddListener(() => {
            onPress.Invoke();
            menu.Close();
        });
    }

    public bool CanOpenDialog() {
        return player.HasControl();
    }
}