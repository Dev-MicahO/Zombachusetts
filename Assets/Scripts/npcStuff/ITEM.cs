using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ITEM : MonoBehaviour, IInteractable
{
    public string uniqueIDForHash;
    
    private GameSession GameSessionScript;
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;

    public TMP_Text dialogueText, nameText;

    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping,isDialogueActive;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if(dialogueData == null)// add something checking if we are paused game rn
            return;

        if(isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }

    }

    void Awake()
    {
        GameSessionScript = GameObject.Find("Gamesession")?.GetComponent<GameSession>();
        if(GameSessionScript.destroyedObjects.Contains(uniqueIDForHash))
        {
            Destroy(gameObject);
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if(isTyping)
        {
            //skip typing animation and show full line
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if(++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            //if another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            //end dialgoue
            EndDialogue();

        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");
        foreach(char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

        GameSessionScript.setItemTrue(dialogueData.npcItem);
        Debug.Log("setItemTrue called with: " + dialogueData.npcItem);
        GameSessionScript.destroyedObjects.Add(uniqueIDForHash);
        Destroy(gameObject);

    }

}
