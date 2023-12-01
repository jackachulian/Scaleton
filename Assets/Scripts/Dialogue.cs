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


    public bool dialoguePlaying {get; private set;}

    bool typing;
    bool waitingForInput;
    // If the player can currently advance the cutscene by inputting. may be set to false during cutscenes with auto-dialogue.
    bool advancable = true;

    private GameObject[] disableDuringDialogue;

    private void Start() {
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
    }

    void Update(){
        if (!advancable) return;

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

        dialoguePlaying = true;

         // This keeps the dialogue from permenantly erasing names from NPC dialogue objects
        var tempL = new string[l.Length];
        for(int i = 0; i < l.Length; i++){
            tempL[i] = l[i];
        }

        gameObject.SetActive(true);
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
        foreach (var obj in disableDuringDialogue) obj.SetActive(false);
        lines = tempL;
        textComponent.text = string.Empty;
        MenuManager.player.DisableControl();
        index = 0;
        ParseLine();
    }

    private void ParseLine() {
        var line = lines[index];
        if (line.Length == 0) {
            NextLine();
            return;
        }

        if (line[0] == '/') {
            var args = line.Split();
            var cmd = args[0].Substring(1);
            
            if (cmd == "wait") {
                try {
                    float delay = float.Parse(args[1]);
                    StartCoroutine(NextLineAfterDelay(delay));
                    if (!typing && !waitingForInput) {
                        dialogueBackground.enabled = false;
                        nameBox.SetActive(false);
                    }
                    return;
                } catch (FormatException) {
                    Debug.LogError("Invalid wait delay: "+args[1]);
                    NextLine();
                    return;
                }
            } 

            else if (cmd == "respawnitems") {
                MenuManager.player.GetCurrentRoom().RespawnItems();
            }
            
            else if (cmd == "setbrightness") {
                MenuManager.player.GetCurrentRoom().SetBrightness(float.Parse(args[1]));
            }

            else if (cmd == "hidebossui") {
                MenuManager.bossUI.gameObject.SetActive(false);
            }

            else if (cmd == "setadvancable") {
                advancable = bool.Parse(args[1]);
            }

            else if (cmd == "fadetowhite") {
                float fadeInTime = args.Length > 1 ? float.Parse(args[1]) : 0.375f;
                float fadeOutTime = args.Length > 2 ? float.Parse(args[2]) : 0.375f;
                string fadeType = args.Length > 3 ? args[3] : "easeIn";

                TransitionManager.Transition(() => NextLine(), Color.white, fadeInTime, fadeOutTime, fadeType);
            }

            else {
                if (Cutscene.current != null) {
                    bool readyForNextLine = Cutscene.current.ParseCommand(cmd, args);
                    if (readyForNextLine) NextLine();
                } else {
                    Debug.LogError("Unknown dialogue cmd: \""+cmd+"\"");
                    NextLine();
                } 
                return;
            }

            // If no other commands started a line or need to wait for another function to start the next line,
            // start the next line here.
            NextLine();
            return;
        } 
        
        // if no function, check for name brackets
        else if (line[0] == '[') {
            int closeBracketIndex = line.IndexOf(']');
            string name = line.Substring(1,closeBracketIndex-1);
            lines[index] = line.Substring(closeBracketIndex+2);
            nameBox.SetActive(true);
            nameLabel.text = name;
            StartCoroutine(TypeLine());
        }

        // otherwise, this is a normal line, type it out
        else {
            nameBox.SetActive(false);
            StartCoroutine(TypeLine());
        }

        // after all this logic, if not typing anything, waiting, or parsing next line,
        // hide the dialogue and name box if not typing, as something is likely being waited for
        if (!typing && !waitingForInput) {
            dialogueBackground.enabled = false;
            nameBox.SetActive(false);
        }
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
        if (typing) {
            Debug.LogError("Tried to start new line while typing");
            return;
        }
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
        dialoguePlaying = false;
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
