using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public Image dialogueBackground;
    public TextMeshProUGUI textComponent;
    public GameObject nameBox;
    public TMP_Text nameLabel;
    private string[] lines;
    public float speed;
    private int index;

    bool typing;
    bool waitingForInput;

    private GameObject[] disableDuringDialogue;

    private void Start() {
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
    }

    void Update(){
        if (Input.GetButtonDown("Interact") || Input.GetButtonDown("Cancel"))
        {
            if (typing) {
                typing = false;
                waitingForInput = true;
                textComponent.text = lines[index];
            } else if (waitingForInput) {
                NextLine();
            }
        }
    }

    public void StartDialogue(string[] l){
        if (l.Length == 0) {
            Debug.LogWarning("Empty dialogue was played");
            return;
        }

        gameObject.SetActive(true);
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
        foreach (var obj in disableDuringDialogue) obj.SetActive(false);
        lines = l;
        textComponent.text = string.Empty;
        MenuManager.player.DisableControl();
        index = 0;
        ParseLine();
    }

    private void ParseLine() {
        var line = lines[index];
        if (line.Length == 0) {
            NextLine();
        }

        if (line[0] == '/') {
            var args = line.Split();
            var cmd = args[0].Substring(1);
            
            if (cmd == "wait") {
                try {
                    float delay = float.Parse(args[1]);
                    StartCoroutine(NextLineAfterDelay(delay));
                } catch (FormatException) {
                    Debug.LogError("Invalid wait delay: "+args[1]);
                    NextLine();
                }
            } else {
                if (Cutscene.current != null) {
                    bool readyForNextLine = Cutscene.current.ParseCommand(cmd, args);
                    if (readyForNextLine) NextLine();
                } else {
                    Debug.LogError("Unknown dialogue cmd: \""+cmd+"\"");
                    NextLine();
                } 
            }
        } 
        
        else if (line[0] == '[') {
            int closeBracketIndex = line.IndexOf(']');
            string name = line.Substring(1,closeBracketIndex);
            lines[index] = line.Substring(closeBracketIndex+1);
            nameBox.SetActive(true);
            nameLabel.text = name;
        }

        else {
            nameBox.SetActive(false);
            StartCoroutine(TypeLine());
        }

        if (!typing && !waitingForInput) dialogueBackground.enabled = false;
    }

    IEnumerator TypeLine(){
        dialogueBackground.enabled = true;
        typing = true;
        foreach (char c in lines[index].ToCharArray()){
            if (!typing) break;
            textComponent.text += c;
            yield return new WaitForSeconds(speed);
        }
        typing = false;
        waitingForInput = true;
    }

    public IEnumerator NextLineAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        NextLine();
    }

    public IEnumerator NextLineOnceTrue(Func<bool> func) {
        while (func.Invoke() == false) {
            yield return null;
        }
        NextLine();
    }

    public void NextLine(){
        if (typing) return;
        waitingForInput = false;
        if(index < lines.Length - 1){
            index++;
            textComponent.text = string.Empty;
            ParseLine();
        }
        else{
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        gameObject.SetActive(false);
        foreach (var obj in disableDuringDialogue) obj.SetActive(true);
        if (Cutscene.current != null) Cutscene.current.CutsceneEnded();
        if (MenuManager.openMenus.Count > 0)
        {
            MenuManager.ShowHiddenMenus();
        } else {
            MenuManager.player.EnableControl();
        }
    }
}
