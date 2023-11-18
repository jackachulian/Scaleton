using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    private string[] lines;
    public float speed;
    private int index;

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private GameObject[] disableDuringDialogue;

    void Reset(){
        if (!player) player = GameObject.Find("Player").GetComponent<PlayerController>();
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
        foreach (var obj in disableDuringDialogue) obj.SetActive(false);
        lines = l;
        textComponent.text = string.Empty;
        player.DisableControl();
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
        if (Menu.openMenus.Count > 0)
        {
            for (int i = 0; i < Menu.openMenus.Count; i++) {
                Menu.openMenus[i].Show();
            }
        } else {
            player.EnableControl();
        }
    }
}
