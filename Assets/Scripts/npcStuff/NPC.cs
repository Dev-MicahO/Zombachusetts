using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour, IInteractable
{
    public string uniqueIDForHash;
    
    private GameSession GameSessionScript;

    private string[] activeLines; 
    private bool[] activeAutoProgressLines;

    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;

    public TMP_Text dialogueText, nameText;

    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping,isDialogueActive;

    void Awake()
    {
        GameSessionScript = GameObject.Find("Gamesession")?.GetComponent<GameSession>();
        if(GameSessionScript.destroyedObjects.Contains(uniqueIDForHash))
        {
            Destroy(gameObject);
        }
    }

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
            SetActiveLines();
            StartDialogue();
        }

    }

    void SetActiveLines()
    {
        if(string.IsNullOrEmpty(dialogueData.npcItem) || GameSessionScript == null)
        {
            Debug.Log("Item was null");
            activeLines = dialogueData.dialogueLines;
            activeAutoProgressLines = dialogueData.autoProgressLines;
        }
        
        else if(GameSessionScript.getItemStatus(dialogueData.npcItem))
        {
            Debug.Log("Destorying dialogue triggered");
            activeLines = dialogueData.destroydialogueLines;
            activeAutoProgressLines = dialogueData.destroyAutoProgressLines;
        }
        else
        {
            Debug.Log("Normal Dialgoue triggered");
            activeLines = dialogueData.dialogueLines;
            activeAutoProgressLines = dialogueData.autoProgressLines;
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
            dialogueText.SetText(activeLines[dialogueIndex]);
            isTyping = false;
        }
        else if(++dialogueIndex < activeLines.Length)
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
        foreach(char letter in activeLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(activeAutoProgressLines.Length > dialogueIndex && activeAutoProgressLines[dialogueIndex])
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
        Debug.Log("trying to get status");
        if(GameSessionScript.getItemStatus(dialogueData.npcItem))
        {
            GameSessionScript.setPartyMemberTrue(nameText.text);
            GameSessionScript.destroyedObjects.Add(uniqueIDForHash);
            Destroy(gameObject);
        }
    }

}
