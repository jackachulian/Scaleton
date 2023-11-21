using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    private string[] lines;
    public float speed;
    private int index;

    private GameObject[] disableDuringDialogue;

    private void Start() {
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
    }

    void Update(){
        if (Input.GetButtonDown("Interact") || Input.GetButtonDown("Cancel"))
        {
            if(textComponent.text == lines[index]){
                NextLine();
            }
            else{
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialogue(string[] l){
        gameObject.SetActive(true);
        if (disableDuringDialogue == null) disableDuringDialogue = GameObject.FindGameObjectsWithTag("DisableDuringDialogue");
        foreach (var obj in disableDuringDialogue) obj.SetActive(false);
        lines = l;
        textComponent.text = string.Empty;
        MenuManager.player.DisableControl();
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine(){
        foreach (char c in lines[index].ToCharArray()){
            textComponent.text += c;
            yield return new WaitForSeconds(speed);
        }
    }

    void NextLine(){
        if(index < lines.Length - 1){
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else{
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        gameObject.SetActive(false);
        foreach (var obj in disableDuringDialogue) obj.SetActive(true);
        if (MenuManager.openMenus.Count > 0)
        {
            MenuManager.ShowHiddenMenus();
        } else {
            MenuManager.player.EnableControl();
        }
    }
}
