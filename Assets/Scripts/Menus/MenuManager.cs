using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    [SerializeField] private Menu emptyMenuPrefab;

    [SerializeField] private Button choiceButtonPrefab;

    public PlayerController player {get; private set;}

    public static Dialogue globalDialogue {get; private set;}

    [SerializeField] private Dialogue dialogueBox;

    private void Start() {
        player = FindObjectOfType<PlayerController>();
        globalDialogue = dialogueBox;
    }

    public Menu CreateDialogMenu() {
        if (!CanOpenDialog()) {
            Debug.LogWarning("Dialog opened while player has control, or at a time dialog shouldn't be openable");
        }
        Menu menu = Instantiate(emptyMenuPrefab, transform);
        foreach (Transform child in menu.transform) {
            Destroy(child.gameObject);
        }
        menu.Show();
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

    public static void StartDialogue(string[] lines) {
        globalDialogue.StartDialogue(lines);
    }
}