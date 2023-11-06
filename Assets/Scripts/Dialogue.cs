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

    void Start(){
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
        lines = l;
        textComponent.text = string.Empty;
        player.charState = PlayerController.CharState.DISABLED;
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
            gameObject.SetActive(false);
            player.charState = PlayerController.CharState.NORMAL;
        }
    }
}
